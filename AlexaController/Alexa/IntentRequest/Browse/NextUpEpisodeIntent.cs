using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class NextUpEpisodeIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestContextData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;

            IDataSource aplDataSource;
            IDataSource aplaDataSource;

            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session.User);

            if (nextUpEpisode is null)
            {
                aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource("There doesn't seem to be a new episode available.", "/particles");
                aplaDataSource = await APLA_DataSourceManager.Instance.NoNextUpEpisodeAvailable();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                        await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)

                    }
                }, Session);
            }

            //Parental Control check for baseItem
            if (!nextUpEpisode.IsParentalAllowed(Session.User))
            {
                if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                {
                    await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                        $"{Session.User} attempted to view a restricted item.", $"{Session.User} attempted to view {nextUpEpisode.Name}.").ConfigureAwait(false);
                }

                aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {nextUpEpisode.OfficialRating}", "/particles");
                aplaDataSource = await APLA_DataSourceManager.Instance.ParentalControlNotAllowed(nextUpEpisode, Session);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                        await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            if (Session.hasRoom)
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, nextUpEpisode);
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            aplDataSource = await APL_DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(nextUpEpisode, Session);
            aplaDataSource = await APLA_DataSourceManager.Instance.BrowseNextUpEpisode(nextUpEpisode, Session);

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource),
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session)
                }

            }, Session);
        }
    }
}

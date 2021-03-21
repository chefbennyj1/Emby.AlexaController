using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplDataSourceManagement.PropertyModels;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestContextData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;

            //IDataSource aplDataSource;
            //IDataSource aplaDataSource;

            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session.User);

            if (nextUpEpisode is null)
            {
                var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync("There doesn't seem to be a new episode available.", "/particles");
                var aplaDataSource = await DataSourceAudioSpeechPropertiesManager.Instance.NoNextUpEpisodeAvailable();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(genericLayoutProperties, Session),
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

                var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync($"Stop! Rated {nextUpEpisode.OfficialRating}", "/particles");
                var aplaDataSource = await DataSourceAudioSpeechPropertiesManager.Instance.ParentalControlNotAllowed(nextUpEpisode, Session);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(genericLayoutProperties, Session),
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

            var detailLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(nextUpEpisode, Session);
            var aplaDataSource1 = await DataSourceAudioSpeechPropertiesManager.Instance.BrowseNextUpEpisode(nextUpEpisode, Session);

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, detailLayoutProperties);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,

                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource1),
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(detailLayoutProperties, Session)
                }

            }, Session);
        }
    }
}

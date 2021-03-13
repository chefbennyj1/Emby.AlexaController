using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
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
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
                Session.hasRoom = !(Session.room is null);
            }
            catch { }

           
            if (!Session.hasRoom && !Session.supportsApl) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            IDataSource aplDataSource = null;
            IDataSource aplaDataSource = null;
            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session.User);
            
            if (nextUpEpisode is null)
            {
                aplDataSource = await AplObjectDataSourceManager.Instance.GetGenericViewDataSource("There doesn't seem to be a new episode available.", "/particles");
                aplaDataSource = await AplAudioDataSourceManager.Instance.NoNextUpEpisodeAvailable();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                        await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                     
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

                aplDataSource =
                    await AplObjectDataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {nextUpEpisode.OfficialRating}", "/particles");
                aplaDataSource = await AplAudioDataSourceManager.Instance.ParentalControlNotAllowed(nextUpEpisode, Session);
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                        await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
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
                    await Task.Run(() =>
                            AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }
            
            aplDataSource = await AplObjectDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(nextUpEpisode, Session);
            aplaDataSource = await AplAudioDataSourceManager.Instance.BrowseNextUpEpisode(nextUpEpisode, Session);
          
            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session);
            var renderAudioDirective    = await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);
        }
    }
}

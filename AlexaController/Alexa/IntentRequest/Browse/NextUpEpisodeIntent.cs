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
using AlexaController.DataSourceProperties;
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
            }
            catch { }

           
            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

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
                aplDataSource = await AplDataSourceManager.Instance.GetGenericViewDataSource("There doesn't seem to be a new episode available.", "/particles");
                aplaDataSource = await AplaDataSourceManager.Instance.NoNextUpEpisodeAvailable();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<IProperty>(aplDataSource, Session),
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                     
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
                    await AplDataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {nextUpEpisode.OfficialRating}", "/particles");
                aplaDataSource = await AplaDataSourceManager.Instance.ParentalControlNotAllowed(nextUpEpisode, Session);
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<IProperty>(aplDataSource, Session),
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            if (!(Session.room is null))
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
            
            aplDataSource = await AplDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(nextUpEpisode, Session);
            aplaDataSource = await AplaDataSourceManager.Instance.BrowseNextUpEpisode(nextUpEpisode, Session);
          
            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session);
            var renderAudioDirective    = await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);
            
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

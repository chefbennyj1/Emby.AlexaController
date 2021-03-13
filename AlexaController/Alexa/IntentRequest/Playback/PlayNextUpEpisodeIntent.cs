using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;


namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public PlayNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }
        public async Task<string> Response()
        {
            //we need a room object
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            } 
            catch { }
            if (Session.room is null) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request       = AlexaRequest.request;
            var intent        = request.intent;
            var slots         = intent.slots;
            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session?.User);

            IDataSource aplaDataSource = null;

            if (nextUpEpisode is null)
            {
                aplaDataSource = await AplAudioDataSourceManager.Instance.GenericItemDoesNotExists();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                      
                    }
                  
                }, Session);
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(Session, nextUpEpisode)).ConfigureAwait(false);
#pragma warning restore 4014

           
            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            var aplDataSource = await AplObjectDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(nextUpEpisode, Session);
            aplaDataSource = await AplAudioDataSourceManager.Instance.PlayNextUpEpisode(nextUpEpisode, Session);
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                directives = new List<IDirective>()
                {
                    await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session),
                    await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.EmbyApl;

namespace AlexaController.Api.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public PlayNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            //we need a room object
            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            if (Session.room is null) return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session?.User);

            //IDataSource aplaDataSource = null;

            if (nextUpEpisode is null)
            {
                var itemDoesNotExistAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                {
                    SpeechResponseType = SpeechResponseType.NoItemExists
                });
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(itemDoesNotExistAudioProperties)
                    }

                }, Session);
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(Session, nextUpEpisode)).ConfigureAwait(false);
#pragma warning restore 4014


            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            var detailLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(nextUpEpisode, Session);
            var nextUpEpisodeAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.PlayNextUpEpisode,
                item = nextUpEpisode,
                session = Session
            });
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(detailLayoutProperties, Session),
                    await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(nextUpEpisodeAudioProperties)
                }
            }, Session);
        }
    }
}

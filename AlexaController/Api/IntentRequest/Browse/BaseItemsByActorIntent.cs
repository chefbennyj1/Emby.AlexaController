using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Browse
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class BaseItemsByActorIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public BaseItemsByActorIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            await AlexaResponseClient.Instance.PostProgressiveResponse("OK.",
                AlexaRequest.context.System.apiAccessToken, AlexaRequest.request.requestId);
            try
            {
                //TODO: Room validation is throwing an error in this class???? WTF!
                Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }


            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var searchNames = GetActorList(slots);

            var result = ServerDataQuery.Instance.GetItemsByActor(Session.User, searchNames);

            if (result is null || !result.Values.Any())
            {
                var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync("I was unable to find that actor.", "/particles");

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find that actor.",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, Session)
                    }
                }, Session);
            }

            if (Session.hasRoom)
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, result.Keys.FirstOrDefault().FirstOrDefault());
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            var actors = result.Keys.FirstOrDefault();
            var actorCollection = result.Values.FirstOrDefault();

            var sequenceLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemCollectionSequenceViewPropertiesAsync(actorCollection);
            var aplaDataSource1 = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.BrowseItemByActor,
                items = actors,
                session = Session
            });

            //TODO: Fix session Update (it is only looking at one actor, might not matter)
            //Update Session
            Session.NowViewingBaseItem = actors[0];
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync<MediaItem>(sequenceLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource1);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);
        }

        private static List<string> GetActorList(Slots slots)
        {
            switch (slots.ActorName.slotValue.type)
            {
                case "Simple": return new List<string>() { slots.ActorName.value };
                case "List": return slots.ActorName.slotValue.values.Select(a => a.value).ToList();
                default: return null;
            }
        }
    }
}

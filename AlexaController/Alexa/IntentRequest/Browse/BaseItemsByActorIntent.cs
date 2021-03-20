using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;

namespace AlexaController.Alexa.IntentRequest.Browse
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
            var searchNames = GetActorList(slots);

            var result = ServerQuery.Instance.GetItemsByActor(Session.User, searchNames);

            if (result is null)
            {
                var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync("I was unable to find that actor.", "/particles");

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
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(genericLayoutProperties, Session)
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

            var sequenceLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetSequenceViewPropertiesAsync(actorCollection);
            var aplaDataSource1 = await DataSourceAudioSpeechPropertiesManager.Instance.BrowseItemByActor(actors);

            //TODO: Fix session Update (it is only looking at one actor, might not matter)
            //Update Session
            Session.NowViewingBaseItem = actors[0];
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(sequenceLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource1);

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

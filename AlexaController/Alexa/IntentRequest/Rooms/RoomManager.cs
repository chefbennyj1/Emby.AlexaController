using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    public interface IRoomContextManager
    {
        Task<string> RequestRoom(IAlexaRequest alexaRequest, IAlexaSession session);
        Room ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session);
        Room GetRoomByName(string name);
    }

    public class RoomManager : IRoomContextManager
    {
        public static IRoomContextManager Instance { get; private set; }
        public RoomManager()
        {
            Instance = this;
        }
        public async Task<string> RequestRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            session.PersistedRequestContextData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    {
                        type = SpeechResponseType.ROOM_CONTEXT, 
                        session = session
                    })
                },
                shouldEndSession = false,
                directives       = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                    {
                        renderDocumentType  = RenderDocumentType.QUESTION_TEMPLATE,
                        HeadlinePrimaryText = "Which room did you want?"

                    }, session)
                }
            }, session);
        }

        public Room GetRoomByName(string name)
        {
            var config = Plugin.Instance.Configuration;
            return ValidateRoomConfiguration(name, config)
                ? config.Rooms.FirstOrDefault(r =>
                    string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase)) : null;
        }

        public Room ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var config = Plugin.Instance.Configuration;

            string room = (slots.Room.value ?? session.room?.Name) ?? request.arguments[1];

            if (string.IsNullOrEmpty(room)) throw new Exception("No room found");

            return ValidateRoomConfiguration(room, config)
                ? config.Rooms.FirstOrDefault(r =>
                    string.Equals(r.Name, room, StringComparison.CurrentCultureIgnoreCase)) : null;

        }

        private static bool ValidateRoomConfiguration(string name, PluginConfiguration config)
        {
            return config.Rooms.Exists(r => string.Equals(r.Name, name,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
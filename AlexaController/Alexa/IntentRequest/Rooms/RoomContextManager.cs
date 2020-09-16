using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    public class RoomContextManager 
    { 
        public string RequestRoom(AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient)
        {
            session.PersistedRequestData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session);

            return responseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.ROOM_CONTEXT, session)
                },
                shouldEndSession = false,
                directives       = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                    {
                        renderDocumentType  = RenderDocumentType.QUESTION_TEMPLATE,
                        HeadlinePrimaryText = "Which room did you want?"

                    }, session)
                }
            }, session.alexaSessionDisplayType);
        }

        public Room ValidateRoom(AlexaRequest alexaRequest, IAlexaSession session)
        {
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var config = Plugin.Instance.Configuration;

            string room = (slots.Room.value ?? session.room?.Name) ?? request.arguments[1];

            if (string.IsNullOrEmpty(room)) return null;

            return ValidateRoomConfiguration(room, config)
                ? config.Rooms.FirstOrDefault(r =>
                    string.Equals(r.Name, room, StringComparison.CurrentCultureIgnoreCase)) : null;

        }

        private static bool ValidateRoomConfiguration(string room, PluginConfiguration config)
        {
            return config.Rooms.Exists(r => string.Equals(r.Name, room,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
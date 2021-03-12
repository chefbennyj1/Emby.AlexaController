using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;

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
            var aplDataSource  = await AplDataSourceManager.Instance.GetGenericViewDataSource("Which room did you want?", "/Question");
            var aplaDataSource = await AplaDataSourceManager.Instance.RoomContext();
            session.PersistedRequestContextData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session, aplDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives       = new List<IDirective>()
                {
                    await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, session),
                    await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
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

        //TODO user may have said "The Family Room", but the room is called "Family Room" - Remove the and try to compare room if needs be
        public Room ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var request = alexaRequest.request;
            var intent  = request.intent;
            var slots   = intent.slots;
            var config  = Plugin.Instance.Configuration;

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
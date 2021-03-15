using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaPresentationManagers;

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
            var aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource("Which room did you want?", "/Question");
            var aplaDataSource = await APLA_DataSourceManager.Instance.RoomContext();
            session.PersistedRequestContextData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session, aplDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await APL_RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, session),
                    await APLA_RenderDocumentManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, session);
        }


        public Room GetRoomByName(string name)
        {
            var config = Plugin.Instance.Configuration;
            return HasRoomConfiguration(name, config)
                ? config.Rooms.FirstOrDefault(r =>
                    string.Equals(r.Name, name, StringComparison.CurrentCultureIgnoreCase)) : null;
        }

        public Room ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var config = Plugin.Instance.Configuration;

            if (!(slots.Room is null))
                return !HasRoomConfiguration(slots.Room.value, config)
                    ? null
                    : config.Rooms.FirstOrDefault(r => string.Equals(r.Name, slots.Room.value, StringComparison.CurrentCultureIgnoreCase));

            if (!(session.room is null)) return session.room;

            if (!(request.arguments is null))
                return !HasRoomConfiguration(request.arguments[1], config)
                    ? null
                    : config.Rooms.FirstOrDefault(r => string.Equals(r.Name, request.arguments[1], StringComparison.CurrentCultureIgnoreCase));

            return null;
        }

        private static bool HasRoomConfiguration(string name, PluginConfiguration config)
        {
            return config.Rooms.Exists(r => string.Equals(r.Name, name,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
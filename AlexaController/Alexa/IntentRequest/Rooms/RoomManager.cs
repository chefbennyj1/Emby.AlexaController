using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    public interface IRoomContextManager
    {
        Task<string> RequestRoom(IAlexaRequest alexaRequest, IAlexaSession session);
        Task<Room> ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session);
        Room GetRoomByName(string name);
    }

    public class RoomContextManager : IRoomContextManager
    {
        public static IRoomContextManager Instance { get; private set; }
        public RoomContextManager()
        {
            Instance = this;
        }
        public async Task<string> RequestRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync("Which room did you want?", "/Question");
            var aplaDataSource          = await DataSourceAudioSpeechPropertiesManager.Instance.RoomContext();
            session.PersistedRequestContextData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(genericLayoutProperties, session),
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)
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

        public async Task<Room> ValidateRoom(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var request = alexaRequest.request;
            var intent  = request.intent;
            var slots   = intent.slots;
            var config  = Plugin.Instance.Configuration;

            //Already have a room marked in the session
            if (!(session.room is null)) return session.room;
            
            //Is a user event (button press)
            if (!(request.arguments is null))
                return !HasRoomConfiguration(request.arguments[1], config)
                    ? null
                    : config.Rooms.FirstOrDefault(r => string.Equals(r.Name, request.arguments[1], StringComparison.CurrentCultureIgnoreCase));
            
            //Room's not mentioned in request
            if (slots.Room.value is null) return null;

            if (!HasRoomConfiguration(slots.Room.value, config))
            {
                await AlexaResponseClient.Instance.PostProgressiveResponse($"Sorry. There is currently no device configuration for {slots.Room.value}.",
                    alexaRequest.context.System.apiAccessToken, alexaRequest.request.requestId).ConfigureAwait(false);
                return null;
            }
            
            var room = config.Rooms.FirstOrDefault(r =>  string.Equals(r.Name, slots.Room.value, StringComparison.CurrentCultureIgnoreCase));
            
            var openEmbySessions = ServerQuery.Instance.GetCurrentSessions().ToList();
            //device needs to be on, and emby must be open and ready for commands
            if (openEmbySessions.Exists(s =>  string.Equals(s.DeviceName, room?.DeviceName, StringComparison.CurrentCultureIgnoreCase)))
            {
                return room;
            }

            //Update the user about the unfortunate state of the emby device being off.
            await AlexaResponseClient.Instance.PostProgressiveResponse($"Sorry. Access to the {room?.Name} device is currently unavailable.",
                alexaRequest.context.System.apiAccessToken, alexaRequest.request.requestId).ConfigureAwait(false);
            return null;

        }

        private static bool HasRoomConfiguration(string name, PluginConfiguration config)
        {
            return config.Rooms.Exists(r => string.Equals(r.Name, name,
                StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
﻿using AlexaController.Alexa.ResponseModel;
using AlexaController.Configuration;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Rooms
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
            var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync("Which room did you want?", "/Question");
            var aplaDataSource = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.RoomContext
            });
            session.PersistedRequestData = alexaRequest;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, session),
                    await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource)
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
            ServerController.Instance.Log.Info("Validating Intent Request Room data.");
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var config = Plugin.Instance.Configuration;

            //Already have a room data saved for the session
            ServerController.Instance.Log.Info("Checking Session Room Data.");
            if (!(session.room is null))
            {
                ServerController.Instance.Log.Info("Session Already Contains Room Data. Returning Room Data.");
                return session.room;
            }

            //Is a user event (button press)
            if (!(request.arguments is null))
                return !HasRoomConfiguration(request.arguments[1], config)
                    ? null
                    : config.Rooms.FirstOrDefault(r => string.Equals(r.Name, request.arguments[1], StringComparison.CurrentCultureIgnoreCase));

            //Room's not mentioned in request
            ServerController.Instance.Log.Info("Checking Intent Request Room Data.");
            if (string.IsNullOrEmpty(slots.Room.value))
            {
                ServerController.Instance.Log.Info("Intent does not mention a room.");
                return null;
            }
            
            if (!HasRoomConfiguration(slots.Room.value, config))
            {
                return null;
            }

            var room = config.Rooms.FirstOrDefault(r => string.Equals(r.Name, slots.Room.value, StringComparison.CurrentCultureIgnoreCase));

            var openEmbySessions = ServerDataQuery.Instance.GetCurrentSessions().ToList();
            //device needs to be on, and emby must be open and ready for commands
            if (openEmbySessions.Exists(s => string.Equals(s.DeviceName, room?.DeviceName, StringComparison.CurrentCultureIgnoreCase)))
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
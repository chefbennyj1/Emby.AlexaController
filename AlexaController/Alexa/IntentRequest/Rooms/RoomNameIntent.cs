using AlexaController.Alexa.RequestModel;
using AlexaController.Api;
using AlexaController.Exceptions;
using AlexaController.Session;
using System;
using System.Threading.Tasks;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class RoomNameIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public RoomNameIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;

            // ReSharper disable once TooManyChainedReferences
            var rePromptIntent = Session.PersistedRequestData.request.intent;
            var rePromptIntentName = rePromptIntent.name.Replace("_", ".");

            Room room;

            if (rePromptIntentName != "Rooms.RoomSetupIntent")
            {
                room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session); //returns null or a room

                //if (!Plugin.Instance.Configuration.Rooms.Exists(r => string.Equals(r.Name, room?.Name, StringComparison.InvariantCultureIgnoreCase)))
                if (room is null)
                {
                    throw new DeviceUnavailableException("That rooms device is currently unavailable to display media.");
                }

            }
            else
            {

                await ServerController.Instance.SendMessageToPluginConfigurationPage("RoomSetupIntent", slots.Room.value);

                //Give a Room object with the Setup Name back to the RoomSetupIntent Class through the Session object.
                //Leave it to the  configuration JavaScript to finish saving the new room set up device information.
                room = new Room() { Name = slots.Room.value };
            }

            Session.room = room;
            Session.hasRoom = true;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            //Use Reflection to load the proper Intent class - AlexaController.Alexa.IntentRequest.{intent.Name}
            var type = Type.GetType($"AlexaController.Alexa.IntentRequest.{ rePromptIntentName }");

            //this time we'll be able to give the user what they want because we have a room object.

            try
            {
                return await GetResponseResult(type, Session.PersistedRequestData, Session);
            }
            catch
            {
                throw new Exception("Room Name Error");
            }

        }

        private static async Task<string> GetResponseResult(Type @namespace, IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var paramArgs = session is null
                ? new object[] { alexaRequest } : new object[] { alexaRequest, session };

            var instance = Activator.CreateInstance(@namespace, paramArgs);
            // ReSharper disable once PossibleNullReferenceException
            return await (Task<string>)@namespace.GetMethod("Response")?.Invoke(instance, null);

        }
    }
}

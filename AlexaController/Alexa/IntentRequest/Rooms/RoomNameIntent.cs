using System;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;

// ReSharper disable once TooManyChainedReferences
// ReSharper disable once PossibleNullReferenceException
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomNameIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public RoomNameIntent(IAlexaRequest aR, IAlexaSession s)
        {
            AlexaRequest = aR;
            Session = s;
           
        }

        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
           
            var rePromptIntent     = Session.PersistedRequestData.request.intent;
            var rePromptIntentName = rePromptIntent.name.Replace("_", ".");
            
            Room room = null;
            
            if (rePromptIntentName != "Rooms.RoomSetupIntent")
            {
                try { room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
                if (!Plugin.Instance.Configuration.Rooms.Exists(r => string.Equals(r.Name, room.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                     throw new Exception("That room is currently not configured to show media.");
                }
            }
            else
            {
                EmbyServerEntryPoint.Instance.SendMessageToPluginConfigurationPage("RoomAndDeviceUtility", slots.Room.value);
                //Give a Room object with the Setup Name back to the RoomSetupIntent Class through the Session object.
                //Leave it to the  configuration JavaScript to finish saving the new room set up device information.
                room = new Room(){ Name = slots.Room.value }; 
            }
            
            Session.room = room;
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
            return await (Task<string>)@namespace.GetMethod("Response")?.Invoke(instance, null);

        }
    }
}

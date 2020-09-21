using System;
using System.Threading;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;

// ReSharper disable once TooManyChainedReferences
// ReSharper disable once PossibleNullReferenceException
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomNameIntent //: IIntentResponse
    {
        private IAlexaRequest alexaRequest { get; }
        private IAlexaSession session { get; }
       
        public RoomNameIntent(IAlexaRequest aR, IAlexaSession s)
        {
            alexaRequest = aR;
            session = s;
           
        }
        public string Response()
        //(IAlexaRequest alexaRequest, IAlexaSession session)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var roomName = slots.Room.value;

            var rePromptIntent     = session.PersistedRequestData.request.intent;
            var rePromptIntentName = rePromptIntent.name.Replace("_", ".");
            
            Room room = null;
            
            if (rePromptIntentName != "Rooms.RoomSetupIntent")
            {
                try { room = RoomContextManager.Instance.ValidateRoom(alexaRequest, session); } catch { }
                if (!Plugin.Instance.Configuration.Rooms.Exists(r => string.Equals(r.Name, room.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                     throw new Exception("That room is currently not configured to show media.");
                }
            }
            else
            {
                EmbyServerEntryPoint.Instance.SendMessageToPluginConfigurationPage("RoomAndDeviceUtility", roomName);
                //Give a Room object with the Setup Name back to the RoomSetupIntent Class through the Session object.
                //Leave it to the  configuration JavaScript to finish saving the new room set up device information.
                room = new Room(){ Name = roomName}; 
            }
            
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session);
            
            //Use Reflection to load the proper Intent class - AlexaController.Alexa.IntentRequest.{intent.Name}
            var type = Type.GetType($"AlexaController.Alexa.IntentRequest.{ rePromptIntentName }");

            //this time we'll be able to give the user what they want because we have a room object.

            try
            {
                return GetResponseResult(type, session.PersistedRequestData, session);
            }
            catch 
            {
                return new ErrorHandler().OnError(new Exception("Room Name Error"), alexaRequest, session);
            }

        }

        private static string GetResponseResult(Type @namespace, IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var paramArgs = session is null
                ? new object[] { alexaRequest }
                : new object[] { alexaRequest, session };

            var instance = Activator.CreateInstance(@namespace, paramArgs);
            return (string)@namespace.GetMethod("Response")?.Invoke(instance, null);
        }
    }
}

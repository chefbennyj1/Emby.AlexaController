using System;
using System.Threading;
using AlexaController.Alexa.Errors;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once TooManyChainedReferences
// ReSharper disable once PossibleNullReferenceException
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomNameIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var request = alexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var roomName = slots.Room.value;

            var rePromptIntent     = session.PersistedRequestData.request.intent;
            var rePromptIntentName = rePromptIntent.name.Replace("_", ".");

            var roomManager = new RoomContextManager();
            Room room = null;
            
            if (rePromptIntentName != "Rooms.RoomSetupIntent")
            {
                try { room = roomManager.ValidateRoom(alexaRequest, session); } catch { }
                if (!Plugin.Instance.Configuration.Rooms.Exists(r => string.Equals(r.Name, room.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                     throw new Exception("That room is currently not configured to show media.");
                }
            }
            else
            {
                sessionManager.SendMessageToAdminSessions("RoomAndDeviceUtility", roomName, CancellationToken.None);
                //Give a Room object with the Setup Name back to the RoomSetupIntent Class through the Session object.
                //Leave it to the  configuration JavaScript to finish saving the new room set up device information.
                room = new Room(){ Name = roomName}; 
            }
            
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session);

            var requestHandlerParams = new object[] { session.PersistedRequestData, session, responseClient, libraryManager, sessionManager, userManager };

            //Use Reflection to load the proper Intent class - AlexaController.Alexa.IntentRequest.{intent.Name}
            var type = Type.GetType($"AlexaController.Alexa.IntentRequest.{ rePromptIntentName }");

            //this time we'll be able to give the user what they want because we have a room object.

            try
            {
                return GetAlexaResponseResult(type, requestHandlerParams);
            }
            catch 
            {
                return new ErrorHandler().OnError(new Exception("Room Name Error"), alexaRequest, session, responseClient);
            }

        }

        private string GetAlexaResponseResult(Type @namespace, object[] requestHandlerParams)
        {
            var instance = Activator.CreateInstance(@namespace ?? throw new Exception("Error getting response"));
            var method   = @namespace.GetMethod("Response");
            var response = method?.Invoke(instance, requestHandlerParams);
            return (string)response;
        }
    }
}

using System;
using AlexaController.Alexa.Errors;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomNameIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var request            = alexaRequest.request;
            var rePromptIntent     = session.PersistedRequestData.request.intent;
            //var room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);//request.intent.slots.Room.value;
            var rePromptIntentName = rePromptIntent.name.Replace("_", ".");

            Room room = null;
            try
            {
                room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);
            }
            catch
            {
            }

            if (rePromptIntentName != "Rooms.RoomSetupIntent")
            {
                if (!Plugin.Instance.Configuration.Rooms.Exists(r => string.Equals(r.Name, room.Name, StringComparison.InvariantCultureIgnoreCase)))
                {
                    return ErrorHandler.Instance.OnError(new Exception("That room is currently not configured to show media."), alexaRequest, session, responseClient);
                }
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
                return ErrorHandler.Instance.OnError(new Exception("Room Name Error"), alexaRequest, session, responseClient);
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

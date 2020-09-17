using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class CollectionsIntent  : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            return new LibraryIntentResponseManager("Collections").Response(alexaRequest, session,
                alexa); //responseClient, libraryManager, roomContextManager);
        }
    }
}

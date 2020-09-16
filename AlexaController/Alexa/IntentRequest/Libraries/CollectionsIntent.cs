using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class CollectionsIntent  : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            return new LibraryIntentResponseManager("Collections").Response(alexaRequest, session, responseClient, libraryManager);
        }
    }
}

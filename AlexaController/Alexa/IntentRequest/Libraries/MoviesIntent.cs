using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class MoviesIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            return new LibraryIntentResponseManager("Movies").Response(alexaRequest, session, responseClient, libraryManager);
        }
    }
}

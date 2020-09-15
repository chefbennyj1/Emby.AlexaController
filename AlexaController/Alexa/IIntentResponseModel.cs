using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa
{
    public interface IIntentResponseModel
    {
        string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager);
    }
}

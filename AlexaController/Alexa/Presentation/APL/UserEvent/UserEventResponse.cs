using AlexaController.Api;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.Presentation.APL.UserEvent
{
    public abstract class UserEventResponse
    {
        public abstract string Response(AlexaRequest alexaRequest, ILibraryManager libraryManager,
            IResponseClient responseClient, ISessionManager sessionManager);
    }
}

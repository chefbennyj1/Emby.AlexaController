using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class TvShowsIntent : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            return new LibraryIntentResponseManager("TV Shows").Response(alexaRequest, session,
                alexa); //responseClient, libraryManager, roomContextManager);
        }
    }
}

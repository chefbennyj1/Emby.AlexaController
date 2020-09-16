using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class GoHomeIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "OK"
                },

            });
        }
    }
}

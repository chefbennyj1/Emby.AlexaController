using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;

// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class GoHomeIntent : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            return alexa.ResponseClient.BuildAlexaResponse(new Response()
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

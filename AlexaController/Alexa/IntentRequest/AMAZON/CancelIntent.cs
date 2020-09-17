
// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class CancelIntent : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            return alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Canceling."
                },

            });
        }
    }
}

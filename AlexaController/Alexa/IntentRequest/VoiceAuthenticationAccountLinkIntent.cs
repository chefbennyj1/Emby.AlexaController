using System.Linq;
using System.Threading;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once TooManyChainedReferences
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
    public class VoiceAuthenticationAccountLinkIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var person        = alexaRequest.context.System.person;
            var config        = Plugin.Instance.Configuration;

            if (person is null)
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase             = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR, session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                });
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    return responseClient.BuildAlexaResponse(new Response
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS, session),
                        }
                    });
                }
            }

            //Send Message to Emby plugin UI
            sessionManager.SendMessageToAdminSessions("SpeechAuthentication", person.personId, CancellationToken.None);

            return responseClient.BuildAlexaResponse(new Response
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS, session),
                },
            });
        }
    }
}

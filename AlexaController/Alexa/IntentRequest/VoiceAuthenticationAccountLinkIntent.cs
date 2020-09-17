using System.Linq;
using System.Threading;
using AlexaController.Alexa.IntentRequest.Rooms;
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
    public class VoiceAuthenticationAccountLinkIntent : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            var person        = alexaRequest.context.System.person;
            var config        = Plugin.Instance.Configuration;

            if (person is null)
            {
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
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
                    return alexa.ResponseClient.BuildAlexaResponse(new Response
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
            alexa.SessionManager.SendMessageToAdminSessions("SpeechAuthentication", person.personId, CancellationToken.None);

            return alexa.ResponseClient.BuildAlexaResponse(new Response
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

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
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }
        public VoiceAuthenticationAccountLinkIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            var person        = AlexaRequest.context.System.person;
            var config        = Plugin.Instance.Configuration;

            if (person is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase             = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR, Session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                });
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    return Alexa.ResponseClient.BuildAlexaResponse(new Response
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS, Session),
                        }
                    });
                }
            }

            //Send Message to Emby plugin UI
            Alexa.SessionManager.SendMessageToAdminSessions("SpeechAuthentication", person.personId, CancellationToken.None);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS, Session),
                },
            });
        }
    }
}

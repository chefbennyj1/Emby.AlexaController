using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
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
        
        public VoiceAuthenticationAccountLinkIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            ;
            Session = session;
            ;
        }
        public string Response()
        {
            var person        = AlexaRequest.context.System.person;
            var config        = Plugin.Instance.Configuration;

            if (person is null)
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase             = SpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR, Session),
                        speechType = SpeechType.APOLOGETIC,
                    },
                });
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    return ResponseClient.Instance.BuildAlexaResponse(new Response
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS, Session),
                        }
                    });
                }
            }

            //Send Message to Emby plugin UI
            Task.Factory.StartNew(() => EmbyServerEntryPoint.Instance.SendMessageToConfiguration("SpeechAuthentication", person.personId));

            return ResponseClient.Instance.BuildAlexaResponse(new Response
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS, Session),
                },
            });
        }
    }
}

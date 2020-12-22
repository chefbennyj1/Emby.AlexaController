using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;


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
            Session = session;
        }
        public async Task<string> Response()
        {
            var context       = AlexaRequest.context;
            var person        = context.System.person;
            var config        = Plugin.Instance.Configuration;

            if (person is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase             = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                        {
                            type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR, 
                            session = Session
                        }),
                        
                    },
                }, Session);
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    return await ResponseClient.Instance.BuildAlexaResponse(new Response
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                            {
                                type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS, 
                                session = Session
                            }),
                        }
                    }, Session);
                }
            }

#pragma warning disable 4014
            Task.Run(() => EmbyServerEntryPoint.Instance.SendMessageToPluginConfigurationPage("SpeechAuthentication", person.personId));
#pragma warning restore 4014

            return await ResponseClient.Instance.BuildAlexaResponse(new Response
            {
                shouldEndSession = true,
                SpeakUserName = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    {
                        type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS, 
                        session  = Session
                    }),
                },
            }, Session);
        }
    }
}

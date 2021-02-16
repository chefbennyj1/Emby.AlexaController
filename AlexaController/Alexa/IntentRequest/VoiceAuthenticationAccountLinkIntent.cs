using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.APLA.Filters;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
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
                    //outputSpeech = new OutputSpeech()
                    //{
                    //    phrase             = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    //    {
                    //        type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR, 
                    //        session = Session
                    //    }),
                        
                    //},
                    directives = new List<IDirective>()
                    {
                        await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(
                            new RenderAudioTemplate()
                            {
                                speechContent = SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR,
                                session = Session,
                                audio = new Audio()
                                {
                                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                    
                                }
                            })
                    }
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
                        //outputSpeech = new OutputSpeech()
                        //{
                        //    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                        //    {
                        //        type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS, 
                        //        session = Session
                        //    }),
                        //}
                        directives = new List<IDirective>()
                        {
                        await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(
                            new RenderAudioTemplate()
                            {
                                speechContent = SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_EXISTS,
                                session = Session,
                                audio = new Audio()
                                {
                                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                    
                                }
                            })
                        }
                    }, Session);
                }
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.SendMessageToPluginConfigurationPage("SpeechAuthentication", person.personId));
#pragma warning restore 4014

            return await ResponseClient.Instance.BuildAlexaResponse(new Response
            {
                shouldEndSession = true,
                SpeakUserName = true,
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                //    {
                //        type = SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS, 
                //        session  = Session
                //    }),
                //},
                directives = new List<IDirective>()
                {
                    await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(
                        new RenderAudioTemplate()
                        {
                            speechContent = SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS,
                            session = Session,
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        })
                }

            }, Session);
        }
    }
}

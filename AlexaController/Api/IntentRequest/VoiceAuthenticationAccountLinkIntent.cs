using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.EmbyApl;

namespace AlexaController.Api.IntentRequest
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class VoiceAuthenticationAccountLinkIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public VoiceAuthenticationAccountLinkIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var context = AlexaRequest.context;
            var person = context.System.person;
            var config = Plugin.Instance.Configuration;

            if (person is null)
            {
                var voiceAuthenticationLinkErrorAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                {
                    SpeechResponseType = SpeechResponseType.VoiceAuthenticationAccountLinkError
                });
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(voiceAuthenticationLinkErrorAudioProperties)
                    }
                }, Session);
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    var voiceAuthenticationProfileExistsAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                    {
                        SpeechResponseType = SpeechResponseType.VoiceAuthenticationExists,
                        session = Session
                    });
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response
                    {
                        shouldEndSession = true,

                        directives = new List<IDirective>()
                        {
                            await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(voiceAuthenticationProfileExistsAudioProperties)
                        }
                    }, Session);
                }
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.SendMessageToPluginConfigurationPage("SpeechAuthentication", person.personId));
#pragma warning restore 4014

            var voiceAuthenticationLinkSuccessAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.VoiceAuthenticationAccountLinkSuccess,
                session = Session
            });

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response
            {
                shouldEndSession = true,

                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(voiceAuthenticationLinkSuccessAudioProperties)
                }

            }, Session);
        }
    }
}

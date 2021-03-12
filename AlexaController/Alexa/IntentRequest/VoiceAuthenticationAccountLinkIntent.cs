using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;


namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
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
            var context       = AlexaRequest.context;
            var person        = context.System.person;
            var config        = Plugin.Instance.Configuration;

            IDataSource aplaDataSource = null;

            if (person is null)
            {
                aplaDataSource = await AplaDataSourceManager.Instance.VoiceAuthenticationAccountLinkError();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            if (config.UserCorrelations.Any())
            {
                if (config.UserCorrelations.Exists(p => p.AlexaPersonId == person.personId))
                {
                    aplaDataSource = await AplaDataSourceManager.Instance.VoiceAuthenticationExists(Session);
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                        }
                    }, Session);
                }
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.SendMessageToPluginConfigurationPage("SpeechAuthentication", person.personId));
#pragma warning restore 4014

            aplaDataSource = await AplaDataSourceManager.Instance.VoiceAuthenticationAccountLinkSuccess(Session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response
            {
                shouldEndSession = true,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }

            }, Session);
        }
    }
}

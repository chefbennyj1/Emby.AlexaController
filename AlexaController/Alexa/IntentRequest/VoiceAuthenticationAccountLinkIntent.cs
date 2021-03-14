using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AlexaController.Alexa.IntentRequest
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

            IDataSource aplaDataSource;

            if (person is null)
            {
                aplaDataSource = await AplAudioDataSourceManager.Instance.VoiceAuthenticationAccountLinkError();
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
                    aplaDataSource = await AplAudioDataSourceManager.Instance.VoiceAuthenticationExists(Session);
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

            aplaDataSource = await AplAudioDataSourceManager.Instance.VoiceAuthenticationAccountLinkSuccess(Session);

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

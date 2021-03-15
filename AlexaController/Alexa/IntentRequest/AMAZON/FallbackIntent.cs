using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaPresentationManagers;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    public class FallbackIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public FallbackIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource("Could you say that again?", "/Question");
            var aplaDataSource = await APLA_DataSourceManager.Instance.NotUnderstood();
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

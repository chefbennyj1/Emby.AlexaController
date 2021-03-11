using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;

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
            var aplDataSource = await AplDataSourceManager.Instance.GetGenericViewDataSource("Could you say that again?", "/Question");
            var aplaDataSource = await AplaDataSourceManager.Instance.NotUnderstood();
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                
                directives = new List<IDirective>()
                {
                    await AplRenderDocumentDirectiveManager.Instance
                        .GetRenderDocumentDirectiveAsync<IProperty>(aplDataSource, Session),
                    await RenderAudioDirectiveManager.Instance
                        .GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

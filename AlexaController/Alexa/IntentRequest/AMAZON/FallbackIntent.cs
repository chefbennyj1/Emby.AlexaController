using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.PresentationManagers;
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
            var aplDataSource = await AplObjectDataSourceManager.Instance.GetGenericViewDataSource("Could you say that again?", "/Question");
            var aplaDataSource = await AplAudioDataSourceManager.Instance.NotUnderstood();
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                
                directives = new List<IDirective>()
                {
                    await AplRenderDocumentDirectiveManager.Instance
                        .GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                    await AplaRenderDocumentDirectiveManager.Instance
                        .GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

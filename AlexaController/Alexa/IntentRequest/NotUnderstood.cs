using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.PresentationManagers;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest
{
    public class NotUnderstood : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NotUnderstood(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
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
                     await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                     await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

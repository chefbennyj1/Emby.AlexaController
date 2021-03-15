using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaPresentationManagers;

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
            var aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource("Could you say that again?", "/Question");
            var aplaDataSource = await APLA_DataSourceManager.Instance.NotUnderstood();
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                     await APL_RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),
                     await APLA_RenderDocumentManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

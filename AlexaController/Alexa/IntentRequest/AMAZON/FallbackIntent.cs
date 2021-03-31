using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    // ReSharper disable once UnusedType.Global
    public class FallbackIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public FallbackIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync("Could you say that again?", "/Question");
            var aplaDataSource = await DataSourcePropertiesManager.Instance.GetSpeechResponseProperties(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.NotUnderstood
            });
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(genericLayoutProperties, Session),
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)
                }
            }, Session);
        }
    }
}

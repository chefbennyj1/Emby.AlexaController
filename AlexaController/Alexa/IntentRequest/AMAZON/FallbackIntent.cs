using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.APLA.Filters;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
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
            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                
                directives = new List<IDirective>()
                {
                    await RenderDocumentManager.Instance
                        .GetRenderDocumentDirectiveAsync(new InternalRenderDocumentQuery()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, Session),
                    await RenderAudioManager.Instance
                        .GetAudioDirectiveAsync(new InternalRenderAudioQuery()
                        {
                            speechContent = SpeechContent.NOT_UNDERSTOOD,
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

using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest
{
    public class NotUnderstood : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NotUnderstood(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                     await RenderDocumentBuilder.Instance
                        .GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, Session),
                     await RenderAudioBuilder.Instance
                         .GetAudioDirectiveAsync(new RenderAudioTemplate()
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

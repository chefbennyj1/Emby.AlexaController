using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
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
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                     await RenderDocumentDirectiveManager.Instance
                        .GetRenderDocumentDirectiveAsync(new RenderDocumentQuery()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, Session),
                     await AudioDirectiveManager.Instance
                         .GetAudioDirectiveAsync(new AudioDirectiveQuery()
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

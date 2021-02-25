using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    public class HelpIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public HelpIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Welcome to help. Swipe left to get started",
                },
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(new InternalRenderDocumentQuery()
                    {
                        renderDocumentType = RenderDocumentType.HELP

                    }, Session)
                    
                }
            }, Session);
        }
    }
}

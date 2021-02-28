using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
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
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Welcome to home theater help. In a moment, swipe left to follow the help instructions.",
                },
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentQuery()
                    {
                        renderDocumentType = RenderDocumentType.HELP

                    }, Session)
                    
                }
            }, Session);
        }
    }
}

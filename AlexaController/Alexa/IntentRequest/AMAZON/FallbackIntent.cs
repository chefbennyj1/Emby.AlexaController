using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;

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
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = false,
                outputSpeech = new OutputSpeech()
                {
                    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    {
                        type = SpeechResponseType.NOT_UNDERSTOOD, 
                        session = Session
                    })
                },
                
                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance
                        .GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, Session)
                }
            }, Session.alexaSessionDisplayType);
        }
    }
}

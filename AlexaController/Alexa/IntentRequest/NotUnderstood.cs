using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;

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
        public string Response()
        {
            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = false,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"I misunderstood what you said. {OutputSpeech.InsertStrengthBreak(StrengthBreak.weak)} " +
                             $"{OutputSpeech.SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low)}",
                    semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                },
                
                directives = new List<IDirective>()
                {
                     RenderDocumentBuilder.Instance
                        .GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, Session)
                }
            }, Session.alexaSessionDisplayType);
        }
    }
}

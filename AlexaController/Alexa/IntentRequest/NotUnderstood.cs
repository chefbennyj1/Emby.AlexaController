using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest
{
    public class NotUnderstood : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = false,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"I misunderstood what you said. {OutputSpeech.InsertStrengthBreak(StrengthBreak.weak)} " +
                             $"{OutputSpeech.SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low)}",
                    semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                },
                
                directives = new List<Directive>()
                {
                     RenderDocumentBuilder.Instance
                        .GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.NOT_UNDERSTOOD
                        }, session)
                }
            }, session.alexaSessionDisplayType);
        }
    }
}

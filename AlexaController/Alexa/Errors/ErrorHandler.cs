using System;
using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;

// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Errors
{
    
    public interface IErrorHandler
    {
        string OnError(Exception exception, IAlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient);
    }

    public class ErrorHandler : IErrorHandler
    {
        public string OnError(Exception exception, IAlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient)
        {
            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{OutputSpeech.SayWithEmotion(exception.Message, Emotion.excited, Intensity.low)}",
                    semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                },

                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance
                        .GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = exception.Message

                        }, session)
                }
            }, session.alexaSessionDisplayType);
        }
    }
}

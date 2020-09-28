using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;

// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Exceptions
{
    public interface IErrorHandler
    {
        Task<string> OnError(Exception exception, IAlexaRequest alexaRequest, IAlexaSession session);
    }

    public class ErrorHandler : IErrorHandler
    {
        public async Task<string> OnError(Exception exception, IAlexaRequest alexaRequest, IAlexaSession session)
        {
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{OutputSpeech.SayWithEmotion(exception.Message, Emotion.excited, Intensity.low)}",
                },

                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance
                        .GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = exception.Message

                        }, session)
                }
            }, session.alexaSessionDisplayType);
        }
    }
}

using System;
using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once MethodNameNotMeaningful

namespace AlexaController.Alexa.Errors
{
    
    public interface IErrorHandler
    {
        string OnError(Exception exception, AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient);
    }

    public class ErrorHandler : IServerEntryPoint, IErrorHandler
    {
        private ILibraryManager LibraryManager { get; }
        private ISessionManager SessionManager { get; }
        private IUserManager UserManager       { get; }
        public static IErrorHandler Instance   { get; private set; }

        public ErrorHandler(ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            SessionManager = sessionManager;
            LibraryManager = libraryManager;
            UserManager    = userManager;
            Instance       = this;
        }
        public string OnError(Exception exception, AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient)
        {
            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{OutputSpeech.SayWithEmotion(exception.Message, Emotion.excited, Intensity.low)}",
                    semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                },

                directives = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance
                        .GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = exception.Message

                        }, session)
                }
            }, session.alexaSessionDisplayType);
        
        }

        public void Dispose()
        {
            
        }

        public void Run()
        {
            
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Presentation.APL.UserEvent.Sequence.onScroll
{
    public class SequenceOnScroll : IUserEventResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, AlexaEntryPoint alexa)//IResponseClient responseClient, ISessionManager sessionManager)
        {
            var arguments = alexaRequest.request.arguments;

            return alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    new Directive()
                    {
                        type = "Alexa.Presentation.APL.ExecuteCommands",
                        token = arguments[1],
                        commands = new List<ICommand>()
                        {
                            new SetValue()
                            {
                                componentId = "header",
                                property    = "headerTitle",
                                value       = "I changed on scroll"
                            }
                        }
                    }
                }
            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
        }
    }
}

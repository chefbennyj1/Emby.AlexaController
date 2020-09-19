using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.Sequence.onScroll
{
    public class SequenceOnScroll : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        
        public SequenceOnScroll(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
            ;
        }
        public string Response()
        {
            var arguments = AlexaRequest.request.arguments;

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
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

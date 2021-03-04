using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseModel;
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
        }
        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var arguments = request.arguments;

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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
            }, null);
        }
    }
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.Video.End
{
    public class VideoOnEnd : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaEntryPoint Alexa { get; }

        public VideoOnEnd(IAlexaRequest alexaRequest, AlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
        }
        public string Response()
        {
            var arguments = AlexaRequest.request.arguments;
            
            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
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
                                componentId = "backdropOverlay",
                                property    = "source",
                                value       = arguments[2]
                            },
                            new SetValue()
                            {
                                componentId = "backdropOverlay",
                                property    = "opacity",
                                value       = 1
                            },
                            new SetValue()
                            {
                                componentId = "backdropOverlay",
                                property    = "overlayColor",
                                value       = "rgba(0,0,0,0.55)"
                            }
                        }
                    }
                }
            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
        }
    }
}

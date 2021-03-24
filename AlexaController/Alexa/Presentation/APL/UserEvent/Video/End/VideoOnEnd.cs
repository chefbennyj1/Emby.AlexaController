using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.Video.End
{
    // ReSharper disable once UnusedType.Global
    public class VideoOnEnd : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public VideoOnEnd(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            var request   = AlexaRequest.request;
            var arguments = request.arguments;
            var session   = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    new ExecuteCommandsDirective()
                    {
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
            }, session);
        }
    }
}

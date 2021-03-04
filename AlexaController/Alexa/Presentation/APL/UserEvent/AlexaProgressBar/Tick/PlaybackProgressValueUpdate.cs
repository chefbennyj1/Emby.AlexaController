using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.AlexaProgressBar.Tick
{
    public class PlaybackProgressValueUpdate : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public PlaybackProgressValueUpdate(IAlexaRequest alexaRequest)
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
                    new Directive()
                    {
                        type     = "Alexa.Presentation.APL.ExecuteCommands",
                        token    = arguments[1],
                        commands = new List<ICommand>()
                        {
                            new SetValue()
                            {
                                componentId = "playbackProgress",
                                property    = "progressValue",
                                value       = AlexaSessionManager.Instance.GetPlaybackProgressTicks(session)
                            }
                        }
                    }
                }
            }, session);
        }
    }
}

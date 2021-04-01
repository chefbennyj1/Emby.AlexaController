using AlexaController.Alexa;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.UserEvent.Container.Tick
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
            var request = AlexaRequest.request;
            var arguments = request.arguments;
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    new ExecuteCommandsDirective()
                    {
                        //type     = "Alexa.Presentation.APL.ExecuteCommands",
                        token    = arguments[1],
                        commands = new List<ICommand>()
                        {
                            new SetValue()
                            {
                                componentId = "playbackProgress",
                                property    = "progressValue",
                                value       = TimeSpan.FromTicks(AlexaSessionManager.Instance.GetSession(AlexaRequest).PlaybackPositionTicks).TotalMinutes
                            }
                        }
                    }
                }
            }, session);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.AlexaProgressBar.onMount
{
    public class NowPlayingEventUpdateRequest : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public NowPlayingEventUpdateRequest(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            var request   = AlexaRequest.request;
            var arguments = request.arguments;
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            ServerController.Instance.Log.Info("ProgressBar Update Request!");
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
                                componentId = "currentPlaybackProgress",
                                property = "progressValue",
                                value = TimeSpan.FromTicks(session.PlaybackPositionTicks).TotalMinutes 
                            }
                        }
                    }
                }
            }, session);
        }
    }
}

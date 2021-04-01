using AlexaController.Alexa;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.Directives;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.UserEvent.Container.Mount
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
            var request = AlexaRequest.request;
            var arguments = request.arguments;
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            ServerController.Instance.Log.Info("ProgressBar Update Request!");
            ServerController.Instance.Log.Info($"ProgressBar Update Request SessionId: {session.SessionId}");
            var progressUpdate = TimeSpan.FromTicks(session.PlaybackPositionTicks).TotalSeconds;
            ServerController.Instance.Log.Info($"ProgressBar Update: {progressUpdate}");
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
                                value = progressUpdate
                            }
                        }
                    }
                }
            }, session);
        }
    }
}

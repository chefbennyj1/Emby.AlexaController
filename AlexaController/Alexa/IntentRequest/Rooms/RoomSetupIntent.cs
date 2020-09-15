using System.Collections.Generic;
using System.Threading;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Route("/AddEcho", Description = "Add Echo Device Id To the Room", Verbs = "GET")]
    public class AddEcho : IReturn<bool>
    {
    }

    [Intent]
    public class RoomSetupIntent : IntentResponseModel, IService
    {
        private IJsonSerializer JsonSerializer { get; }
        public RoomSetupIntent(IJsonSerializer json)
        {
            JsonSerializer = json;
        }

        public bool Get(AddEcho request)
        {
            return true;
        }

        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var room = session.room;
            if (room == null)
            {
                session.PersistedRequestData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session);

                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<Directive>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = "Please say the name of the room you want to setup.",

                        }, session)
                    }

                }, session.alexaSessionDisplayType);
            }

            sessionManager.SendMessageToAdminSessions("RoomAndDeviceUtility", room, CancellationToken.None);

            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { room }, and press the \"Create Room button\".",

                }
            }, session.alexaSessionDisplayType);
        }
    }
}

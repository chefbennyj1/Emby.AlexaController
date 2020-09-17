using System.Collections.Generic;
using System.Threading;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Services;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomSetupIntent : IIntentResponse, IService
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            var room = session.room;
            if (room == null)
            {
                session.PersistedRequestData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session);

                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = "Please say the name of the room you want to setup.",

                        }, session)
                    }

                }, session.alexaSessionDisplayType);
            }
            
            var response = alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { room.Name }, and press the \"Create Room button\".",

                }
            }, session.alexaSessionDisplayType);

            session.room = null;

            return response;
        }
    }
}

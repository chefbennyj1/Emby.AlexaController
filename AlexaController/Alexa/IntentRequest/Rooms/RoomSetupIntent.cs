using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;


namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomSetupIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public RoomSetupIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            if (Session.room is null)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session);

                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = "Please say the name of the room you want to setup.",

                        }, Session)
                    }

                }, Session.alexaSessionDisplayType);
            }
            
            var response = await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { Session.room.Name }, and press the \"Create Room button\".",

                }
            }, Session.alexaSessionDisplayType);

            Session.room = null;
            AlexaSessionManager.Instance.UpdateSession(Session);

            return response;
        }
    }
}

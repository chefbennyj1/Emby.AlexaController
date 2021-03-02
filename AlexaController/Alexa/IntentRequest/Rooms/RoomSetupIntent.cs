using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;


namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomSetupIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public RoomSetupIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var request           = AlexaRequest.request;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = Session
            //});

#pragma warning disable 4014
            Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse("One moment please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var room = Session.room;

            if (room is null)
            {
                Session.PersistedRequestContextData = AlexaRequest;

                var dataSource =
                    await DataSourceManager.Instance.GetGenericHeadline(
                        "Please say the name of the room you want to setup.");

                AlexaSessionManager.Instance.UpdateSession(Session, null);



                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session)
                    }

                }, Session);
            }
            
            var response = await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { room.Name }, and press the \"Create Room button\".",

                }
            }, Session);

            Session.room = null;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            return response;
        }
    }
}

using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventReadOverview : IUserEventResponse
    {
        public UserEventReadOverview(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public IAlexaRequest AlexaRequest { get; }
        public async Task<string> Response()
        {
            var request  = AlexaRequest.request;
            var source   = request.source;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = ServerQuery.Instance.GetItemById(source.id);

            
#pragma warning disable 4014
            Task.Run(() =>
                    AlexaResponseClient.Instance.PostProgressiveResponse("One moment please...", AlexaRequest.context.System.apiAccessToken,
                        request.requestId))
                .ConfigureAwait(false);
#pragma warning restore 4014

            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = baseItem.Overview,
                },
                SpeakUserName = false,
                shouldEndSession = null

            }, session); 
        
        }
    }
}

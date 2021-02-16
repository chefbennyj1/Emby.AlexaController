using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;

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

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE,
            //    session = session
            //});
#pragma warning disable 4014
            Task.Run(() =>
                    ResponseClient.Instance.PostProgressiveResponse("One moment please...", AlexaRequest.context.System.apiAccessToken,
                        request.requestId))
                .ConfigureAwait(false);
#pragma warning restore 4014

            
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
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

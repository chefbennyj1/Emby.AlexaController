using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page
{
    public class HelpPager : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public HelpPager(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }

        public async Task<string> Response()
        {
            var request       = AlexaRequest.request;
            var arguments     = request.arguments;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,

                outputSpeech = new OutputSpeech()
                {
                    phrase = arguments[1]
                }

            }, session);
        }
    }
}

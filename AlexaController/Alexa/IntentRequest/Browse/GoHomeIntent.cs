using System.Threading.Tasks;
using AlexaController.Alexa.Model.RequestData;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class GoHomeIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public GoHomeIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "OK"
                },

            }, Session);
        }
    }
}

using System.Threading.Tasks;
using AlexaController.Alexa.Model.RequestData;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class StopIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public StopIntent(IAlexaRequest alexaRequest, IAlexaSession session)
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
                    phrase = "Canceling."
                }
            }, Session);
        }
    }
}
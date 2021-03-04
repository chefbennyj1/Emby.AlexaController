using System;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestModel;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class YesIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public YesIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
           
        }
        public async Task<string> Response()
        {
            throw new NotImplementedException();
        }
    }
}

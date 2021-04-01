using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Session;
using System;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.AMAZON
{
    [Intent]
    // ReSharper disable once UnusedType.Global
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

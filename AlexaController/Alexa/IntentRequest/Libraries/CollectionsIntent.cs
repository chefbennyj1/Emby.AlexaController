using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class CollectionsIntent  : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public CollectionsIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await new LibraryIntentResponse("Collections").Response(AlexaRequest, Session);
        }
    }
}

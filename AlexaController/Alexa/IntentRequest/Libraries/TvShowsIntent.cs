using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class TvShowsIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public TvShowsIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await new LibraryIntentResponse("TV Shows").Response(AlexaRequest, Session);
        }
    }
}

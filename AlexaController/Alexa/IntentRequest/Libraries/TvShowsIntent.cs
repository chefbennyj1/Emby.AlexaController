using AlexaController.Alexa.RequestModel;
using AlexaController.Api;
using AlexaController.Session;
using System.Threading.Tasks;

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    // ReSharper disable once UnusedType.Global
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

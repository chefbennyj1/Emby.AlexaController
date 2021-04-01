using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Session;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Libraries
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class MoviesIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public MoviesIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await new LibraryIntentResponse("Movies").Response(AlexaRequest, Session);
        }
    }
}

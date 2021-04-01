using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Session;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Libraries
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class CollectionsIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
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

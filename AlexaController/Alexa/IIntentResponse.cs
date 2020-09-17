using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa
{
    public interface IIntentResponse
    {
        string Response(IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa); 
    }
}

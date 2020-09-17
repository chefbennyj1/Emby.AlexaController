using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa
{
    public interface IIntentResponse
    {
        IAlexaRequest AlexaRequest { get; }
        IAlexaSession Session      { get; }
        IAlexaEntryPoint Alexa     { get; }
        string Response(); 
    }
}

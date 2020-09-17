using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa
{
    public interface IUserEventResponse 
    {
        IAlexaRequest AlexaRequest { get; }
        IAlexaEntryPoint Alexa { get; }
        string Response(); 
    }
}

using AlexaController.Api;

namespace AlexaController.Alexa
{
    public interface IUserEventResponse 
    {
        IAlexaRequest AlexaRequest { get; }
        string Response(); 
    }
}

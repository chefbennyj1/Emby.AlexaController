using AlexaController.Api;

namespace AlexaController.Alexa
{
    public interface IUserEventResponse
    {
        string Response(IAlexaRequest alexaRequest, AlexaEntryPoint alexa); 
    }
}

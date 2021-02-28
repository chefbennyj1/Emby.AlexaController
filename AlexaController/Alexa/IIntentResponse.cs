using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa
{
    public interface IIntentResponse 
    {
        IAlexaRequest AlexaRequest { get; }
        IAlexaSession Session      { get; }
        Task<string> Response(); 
    }
}

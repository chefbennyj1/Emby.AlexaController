using AlexaController.Api;
using AlexaController.Session;
using System.Threading.Tasks;

namespace AlexaController.Alexa
{
    public interface IIntentResponse
    {
        IAlexaRequest AlexaRequest { get; }
        IAlexaSession Session { get; }
        Task<string> Response();
    }
}

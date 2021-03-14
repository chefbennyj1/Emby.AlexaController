using AlexaController.Api;
using System.Threading.Tasks;

namespace AlexaController.Alexa
{
    public interface IUserEventResponse
    {
        IAlexaRequest AlexaRequest { get; }
        Task<string> Response();
    }
}

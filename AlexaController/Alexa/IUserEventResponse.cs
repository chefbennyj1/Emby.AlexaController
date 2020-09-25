using System.Threading.Tasks;
using AlexaController.Api;

namespace AlexaController.Alexa
{
    public interface IUserEventResponse 
    {
        IAlexaRequest AlexaRequest { get; }
        Task<string> Response(); 
    }
}

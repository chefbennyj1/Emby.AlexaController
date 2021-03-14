using AlexaController.Alexa.Presentation.APL;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation
{
    public interface IDocument
    {
        string type { get; }
        string version { get; }
        Settings settings { get; }
        List<Resource> resources { get; }
        IMainTemplate mainTemplate { get; }
    }
}
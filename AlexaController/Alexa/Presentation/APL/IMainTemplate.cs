using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL
{
    public interface IMainTemplate
    {
        List<string> parameters { get; }
        List<IItem> items       { get; }
    }

    public class MainTemplate : IMainTemplate
    {
        public List<string> parameters { get; set; }
        public List<IItem> items       { get; set; }
    }
}

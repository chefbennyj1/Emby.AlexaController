using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation
{
    public class MainTemplate : IMainTemplate
    {
        public List<string> parameters { get; set; }
        public List<IComponent> items { get; set; }
        public IComponent item { get; set; }
    }
}

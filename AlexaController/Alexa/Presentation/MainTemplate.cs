using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation
{
    public class MainTemplate : IMainTemplate
    {
        public List<string> parameters { get; set; }
        public List<IItem> items { get; set; }
        public IItem item { get; set; }
    }
}

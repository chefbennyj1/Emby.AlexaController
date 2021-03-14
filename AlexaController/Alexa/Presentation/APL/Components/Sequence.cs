using AlexaController.Alexa.Presentation.APL.Commands;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Sequence : VisualBaseComponent
    {
        public string scrollDirection { get; set; }
        public List<ICommand> onScroll { get; set; }
        public object type => nameof(Sequence);
    }
}



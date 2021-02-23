using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Sequence : VisualBaseItem
    {
        public string scrollDirection { get; set; }
        public List<ICommand> onScroll  { get; set; }
        public object type => nameof(Sequence);
    }
}



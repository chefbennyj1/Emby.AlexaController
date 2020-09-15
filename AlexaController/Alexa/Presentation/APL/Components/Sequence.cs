using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Sequence : Item
    {
        public string scrollDirection { get; set; }
        public List<object> onScroll  { get; set; }

        public object type => nameof(Sequence);
    }
}



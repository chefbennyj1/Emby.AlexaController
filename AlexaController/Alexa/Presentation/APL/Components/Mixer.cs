using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Mixer : Item
    {
        public object type => nameof(Mixer);
        public List<Item> items { get; set; }
    }
    
}

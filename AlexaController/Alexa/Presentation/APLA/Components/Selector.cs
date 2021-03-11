using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Selector : AudioBaseComponent
    {
        public object type => nameof(Selector);
        public List<AudioBaseComponent> items { get; set; }
        public AudioBaseComponent item { get; set; }
        /// <summary>
        /// normal, randomItem, randomData, randomItemRandomData
        /// </summary>
        public string strategy { get; set; } 
    }
}

using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Selector : AudioItem
    {
        public object type => nameof(Selector);
        public List<AudioItem> items { get; set; }
        public AudioItem item { get; set; }
        /// <summary>
        /// normal, randomItem, randomData, randomItemRandomData
        /// </summary>
        public string strategy { get; set; } 
    }
}

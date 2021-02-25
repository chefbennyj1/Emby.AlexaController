using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Selector : AudioBaseItem
    {
        public object type => nameof(Selector);
        public List<AudioBaseItem> items { get; set; }
        public AudioBaseItem item { get; set; }
        /// <summary>
        /// normal, randomItem, randomData, randomItemRandomData
        /// </summary>
        public string strategy { get; set; } 
    }
}

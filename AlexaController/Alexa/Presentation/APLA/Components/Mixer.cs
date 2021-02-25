using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Mixer : AudioBaseItem
    {
        public object type => nameof(Mixer);
        public List<AudioBaseItem> items { get; set; }
        public AudioBaseItem item { get; set; }
        public Strategy strategy { get; set; }
    }

    // ReSharper disable InconsistentNaming
    public enum Strategy
    {
        normal,
        randomItem,
        randomData,
        randomItemRandomData
    }
}

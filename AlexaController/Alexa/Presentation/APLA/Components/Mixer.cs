using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Mixer : AudioItem
    {
        public object type => nameof(Mixer);
        public List<AudioItem> items { get; set; }
        public AudioItem item { get; set; }
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

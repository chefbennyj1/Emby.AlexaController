using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Mixer : AudioBaseComponent
    {
        public object type => nameof(Mixer);
        public List<AudioBaseComponent> items { get; set; }
        public AudioBaseComponent item { get; set; }
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

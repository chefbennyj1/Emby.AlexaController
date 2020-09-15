using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Source
    {
        public string url { get; set; }
        public int repeatCount { get; set; }
    }

    public class Video : Item
    {
        public object type => nameof(Video);
        public List<Source> source { get; set; }
        public bool autoplay       { get; set; }
        public string scale        { get; set; }
        public string audioTrack   { get; set; }
        public List<object> onEnd  { get; set; }
    }
}
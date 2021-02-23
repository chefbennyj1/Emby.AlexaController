using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Source
    {
        public string url { get; set; }
        public int repeatCount { get; set; }
    }

    public class Video : VisualBaseItem
    {
        public object type => nameof(Video);
        public List<Source> source { get; set; }
        public bool autoplay       { get; set; }
        public string scale        { get; set; }
        public string audioTrack   { get; set; }
        public List<ICommand> onEnd  { get; set; }

    }
}
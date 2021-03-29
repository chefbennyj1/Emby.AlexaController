using System.Collections.Generic;

namespace AlexaController.Alexa.RequestModel
{
    public class Context
    {
        public System System { get; set; }
        public AudioPlayer AudioPlayer { get; set; }
        public List<Viewport> Viewports { get; set; }
        public Viewport Viewport { get; set; }
        public Extensions Extensions { get; set; }
    }
}

using System.Collections.Generic;

namespace AlexaController.Alexa.Model.RequestData
{
    public class Context
    {
        public System System { get; set; }
        public AudioPlayer AudioPlayer { get; set; }
        public List<Viewport> Viewports { get; set; }
        public Viewport Viewport { get; set; }
    }
}

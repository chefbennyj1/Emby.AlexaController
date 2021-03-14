using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaBackground : VisualBaseComponent
    {
        public object type => nameof(AlexaBackground);

        public string videoAudioTrack { get; set; } = "none";
        public string backgroundAlign { get; set; }
        public bool backgroundBlur { get; set; }
        public string backgroundImageSource { get; set; }
        public string backgroundScale { get; set; }
        public List<Source> backgroundVideoSource { get; set; }
        public bool colorOverlay { get; set; }
        public bool overlayGradient { get; set; }
        public bool videoAutoPlay { get; set; }

    }
}

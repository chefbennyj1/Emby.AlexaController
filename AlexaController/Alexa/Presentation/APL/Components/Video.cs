﻿using AlexaController.Alexa.Presentation.APL.Commands;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Source
    {
        public string url { get; set; }
        public int repeatCount { get; set; }
    }

    public class Video : VisualBaseComponent
    {
        public object type => nameof(Video);
        public List<Source> source { get; set; }
        public bool autoplay { get; set; }
        public string scale { get; set; }
        public string audioTrack { get; set; }
        public List<ICommand> onEnd { get; set; }

    }
}
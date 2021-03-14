﻿namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaHeadline : VisualBaseComponent
    {
        public object type => nameof(AlexaHeadline);
        public string primaryText { get; set; }
        public string secondaryText { get; set; }
        public string backgroundColor { get; set; }

    }
}
using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaButton : VisualBaseComponent
    {
        public object type => nameof(AlexaButton);
        public SendEvent primaryAction { get; set; }
        public string buttonText       { get; set; }
        public string buttonStyle      { get; set; }
        public string fontSize         { get; set; }
    }
}
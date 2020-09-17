using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class OverlayGradient
    {
        public object type => GetType().Name;
        public List<string> colorRange { get; set; }
        public List<double> inputRange { get; set; }
    }
}

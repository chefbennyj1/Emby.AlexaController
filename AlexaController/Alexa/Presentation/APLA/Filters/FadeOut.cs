using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class FadeOut : Filter
    {
        public string type => nameof(FadeOut);
        public int duration { get; set; }
    }
}

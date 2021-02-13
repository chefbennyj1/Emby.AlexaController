using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class FadeIn : Filter
    {
        public string type => nameof(FadeIn);
        public int duration { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class FadeOut : IFilter
    {
        public object type => nameof(FadeOut);
        public int duration { get; set; }
    }
}

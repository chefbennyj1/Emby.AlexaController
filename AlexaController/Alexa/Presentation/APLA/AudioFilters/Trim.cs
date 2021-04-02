using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.AudioFilters
{
    public class Trim : IFilter
    {
        public object type => nameof(Trim);
        public int start { get; set; }
        public int? end { get; set; }
    }
}

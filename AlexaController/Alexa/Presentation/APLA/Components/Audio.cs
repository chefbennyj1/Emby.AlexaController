using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APLA.Filters;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Audio : AudioItem
    {
        public string duration => "trimToParent";
        public object type => nameof(Audio);
        public string source       { get; set; }
        public List<Filter> filter { get; set; }
    }
}

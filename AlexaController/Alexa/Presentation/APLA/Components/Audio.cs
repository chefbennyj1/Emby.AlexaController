using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APLA.AudioFilters;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Audio : AudioBaseComponent
    {
        public string duration => "trimToParent";
        public object type     => nameof(Audio);
        public string source        { get; set; }
        public List<IFilter> filter { get; set; }
    }
}

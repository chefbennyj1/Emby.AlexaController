using AlexaController.Alexa.Presentation.APL.Components.VisualFilters;
using System.Collections.Generic;


namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Image : VisualBaseComponent
    {
        public object type => nameof(Image);
        public string source { get; set; }
        public string overlayColor { get; set; }
        public string scale { get; set; }
        public Gradient overlayGradient { get; set; }
        public List<IFilter> filters { get; set; }
    }
}
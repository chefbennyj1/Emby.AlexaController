using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components.VisualFilters;


namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Image : VisualBaseItem
    {
        public object type => nameof(Image);
        public string source                   { get; set; }
        public string overlayColor { get; set; }
        public string scale                    { get; set; }
        public Gradient overlayGradient        { get; set; }
        public List<IFilter> filter { get; set; }
    }
}
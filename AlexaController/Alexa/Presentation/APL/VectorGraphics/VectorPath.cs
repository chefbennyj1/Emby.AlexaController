using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class VectorPath : IVectorGraphic
    {
        public object type => "path";
        public string pathData    { get; set; }

        public string source { get; set; }
        public string scale { get; set; }
        public string stroke { get; set; }
        public int strokeWidth { get; set; }
        public string fill { get; set; }
        public List<VectorFilters> filters { get; set; }
    }
}
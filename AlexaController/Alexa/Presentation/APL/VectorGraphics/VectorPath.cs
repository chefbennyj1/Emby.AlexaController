using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class VectorPath : IVectorGraphic
    {
        public object type => "path";
        public string pathData { get; set; }

        public string source { get; set; }
        public string scale { get; set; }
        public string stroke { get; set; }
        public double strokeWidth { get; set; }
        public List<string> strokeDashArray { get; set; }
        public int strokeDashOffset { get; set; }
        public string fill { get; set; }
        public List<VectorFilter> filters { get; set; }
    }
}
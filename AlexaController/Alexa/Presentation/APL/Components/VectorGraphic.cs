using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class VectorGraphic : VisualBaseComponent
    {
        public object type => nameof(VectorGraphic);
        public string source { get; set; }
        public string scale { get; set; }
        public string stroke      { get; set; }
        public int strokeWidth { get; set; }
        public object strokeDashOffset { get; set; }
        public string fill { get; set; }
        public string input { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class VectorGraphic : VisualBaseItem
    {
        public object type => nameof(VectorGraphic);
        public string source { get; set; }
        public string scale { get; set; }
        public string stroke      { get; set; }
        public int strokeWidth { get; set; }
        public string fill { get; set; }
    }
}

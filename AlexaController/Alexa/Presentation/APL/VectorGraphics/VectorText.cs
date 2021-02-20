using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class VectorText : Text, IVectorGraphic
    {
        public new object type => "text";
        public string source { get; set; }
        public string scale { get; set; }
        public string stroke { get; set; }
        public string strokeWidth { get; set; }
        public string fill { get; set; }
        public string textAnchor { get; set; }
        public List<VectorFilters> filters { get; set; }
    }
}

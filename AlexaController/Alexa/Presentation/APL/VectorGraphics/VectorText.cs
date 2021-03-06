﻿using AlexaController.Alexa.Presentation.APL.Components;
using System.Collections.Generic;

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
        public List<VectorFilter> filters { get; set; }
        public int x { get; set; }
        public int y { get; set; }
    }
}

using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class VectorGraphic : VisualItem
    {
        public object type => nameof(VectorGraphic);
        public string source { get; set; }
        public string scale { get; set; }
    }
}
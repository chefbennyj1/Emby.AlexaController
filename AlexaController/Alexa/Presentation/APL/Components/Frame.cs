using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Frame : VisualBaseComponent
    {
        public string backgroundColor { get; set; }
        public string borderRadius    { get; set; }
        public string borderWidth     { get; set; }
        public string borderColor     { get; set; }
        public object type => nameof(Frame);

       
    }
}
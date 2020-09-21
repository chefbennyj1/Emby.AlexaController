using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class Path : Item
    {
        public object type => "path";
        public string pathData    { get; set; }
        public string stroke      { get; set; }
        public string strokeWidth { get; set; }
        public string fill        { get; set; }
    }
}
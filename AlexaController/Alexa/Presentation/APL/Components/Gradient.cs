using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Gradient
    {
        public string type => "linear";
        public List<string> colorRange { get; set; }
        public List<double> inputRange { get; set; }
        public int angle               { get; set; }
    }
}

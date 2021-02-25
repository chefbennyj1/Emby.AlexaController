using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components.FIlters;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Gradient : IFilter
    {
        public string type => nameof(Gradient);
        public GradientOptions gradient { get; set; }
    }

    public class GradientOptions
    {
        public string type { get; set; }
        public List<string> colorRange { get; set; }
        public List<double> inputRange { get; set; }
        public int angle               { get; set; }
    }
}

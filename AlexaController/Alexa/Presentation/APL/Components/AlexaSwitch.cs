using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaSwitch : Item
    {
        public object type => typeof(AlexaSwitch);
        public SendEvent primaryAction { get; set; }
        public bool @checked { get; set; }
    }
}

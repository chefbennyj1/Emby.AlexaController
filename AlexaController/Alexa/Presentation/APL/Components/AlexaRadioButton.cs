using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaRadioButton : VisualBaseComponent
    {
        public object type => typeof(AlexaRadioButton);
        public bool @checked { get; set; }
        public string radioButtonColor { get; set; }
        public SendEvent primaryAction { get; set; }
    }
}

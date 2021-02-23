using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class TouchWrapper : VisualBaseItem
    {
        public ICommand onPress { get; set; }
        public ICommand onDoublePress { get; set; }
        public object type => nameof(TouchWrapper);

    }
}
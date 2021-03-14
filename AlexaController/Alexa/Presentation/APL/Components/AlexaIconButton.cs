namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaIconButton : VisualBaseComponent
    {
        public object type => nameof(AlexaIconButton);
        public object primaryAction { get; set; }
        public string buttonText { get; set; }
        public string buttonStyle { get; set; }
        public string fontSize { get; set; }
        public string vectorSource { get; set; }
        public string buttonSize { get; set; }


    }
}
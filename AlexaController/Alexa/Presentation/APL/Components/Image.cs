namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Image : Item
    {
        public string source                   { get; set; }
        public string overlayColor             { get; set; }
        public string scale                    { get; set; }
        public OverlayGradient overlayGradient { get; set; }
        public object type => nameof(Image);
    }
}
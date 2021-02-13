namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Speech : AudioItem
    {
        public object type => nameof(Speech);
        public string contentType => "SSML";
        public string content { get; set; }
    }
}

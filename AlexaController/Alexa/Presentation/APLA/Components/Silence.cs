namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Silence : AudioBaseComponent
    {
        public object type => nameof(Silence);
        public int duration { get; set; }
    }
}

namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class Repeat : Filter
    {
        public string type => nameof(Repeat);
        public int repeatCount { get; set; }
    }
}

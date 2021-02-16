namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class Repeat : IFilter
    {
        public object type => nameof(Repeat);
        public int repeatCount { get; set; }
    }
}

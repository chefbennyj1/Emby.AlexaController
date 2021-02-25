namespace AlexaController.Alexa.Presentation.APLA.AudioFilters
{
    public class Repeat : IFilter
    {
        public object type => nameof(Repeat);
        public int repeatCount { get; set; }
    }
}

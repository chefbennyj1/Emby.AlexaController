namespace AlexaController.Alexa.Presentation.APLA.AudioFilters
{
    public class Volume : IFilter
    {
        public object type => nameof(Volume);
        public double amount { get; set; }
    }
}

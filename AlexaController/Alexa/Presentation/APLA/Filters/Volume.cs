namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class Volume : IFilter
    {
        public object type => nameof(Volume);
        public double amount { get; set; }
    }
}

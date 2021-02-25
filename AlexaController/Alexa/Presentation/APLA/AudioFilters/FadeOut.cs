namespace AlexaController.Alexa.Presentation.APLA.AudioFilters
{
    public class FadeOut : IFilter
    {
        public object type => nameof(FadeOut);
        public int duration { get; set; }
    }
}

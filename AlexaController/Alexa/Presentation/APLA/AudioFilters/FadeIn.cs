namespace AlexaController.Alexa.Presentation.APLA.AudioFilters
{
    public class FadeIn : IFilter
    {
        public object type => nameof(FadeIn);
        public int duration { get; set; }
    }
}

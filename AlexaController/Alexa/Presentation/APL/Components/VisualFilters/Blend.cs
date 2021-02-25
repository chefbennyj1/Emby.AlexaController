namespace AlexaController.Alexa.Presentation.APL.Components.VisualFilters
{
    public class Blend : IFilter
    {
        public object type => nameof(Blend);
        public string mode { get; set; }
        public int source { get; set; }
        public int destination { get; set; }
    }
}

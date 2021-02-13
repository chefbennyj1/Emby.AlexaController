namespace AlexaController.Alexa.Presentation.APLA.Filters
{
    public class Volume : Filter
    {
        public string type => nameof(Volume);
        /// <summary>
        /// Percent 0% - 100%
        /// </summary>
        public string amount { get; set; }

    }
}

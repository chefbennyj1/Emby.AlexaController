namespace AlexaController.DataSourceProperties.AplaDataSourceProperties
{
    public enum SpeechPrefix
    {
        REPOSE,
        APOLOGETIC,
        COMPLIANCE,
        NONE,
        GREETINGS,
        NON_COMPLIANT,
        DEFAULT
    }
    
    public class SpeechContentProperties : BaseProperties
    {
        public string value { get; set; }
        public string audioUrl { get; set; }
    }
}

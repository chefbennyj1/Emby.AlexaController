using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources;

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
    
    public class SpeechContentProperties : IProperties
    {
        public string value { get; set; }
        public string audioUrl { get; set; }
        public RenderDocumentType documentType { get; set; }
    }
}

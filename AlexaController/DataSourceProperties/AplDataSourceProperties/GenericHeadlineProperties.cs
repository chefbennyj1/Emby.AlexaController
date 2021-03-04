using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.DataSourceProperties.AplDataSourceProperties
{
    public class GenericHeadlineProperties : IProperties
    {
        public string HeadlinePrimaryText { get; set; }
        public RenderDocumentType documentType { get; set; }
        public string url { get; set; }
    }
}

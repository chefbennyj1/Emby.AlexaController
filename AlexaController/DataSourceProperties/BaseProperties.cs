using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.DataSourceProperties
{
    public enum RenderDocumentType
    {
        NONE,
        GENERIC_VIEW,
        ITEM_DETAILS_TEMPLATE,
        ITEM_LIST_SEQUENCE_TEMPLATE,
        APLA,
        HELP,
        ROOM_SELECTION_TEMPLATE,
        VERTICAL_TEXT_LIST_TEMPLATE
    }

    public class BaseProperties : IProperties
    {
        public RenderDocumentType documentType { get; set; }
        public string url { get; set; }
        public string audioUrl { get; set; }
    }
}

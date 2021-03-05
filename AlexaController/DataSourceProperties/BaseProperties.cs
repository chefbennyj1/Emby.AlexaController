using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.DataSourceProperties
{
    public enum RenderDocumentType
    {
        NONE,
        ITEM_DETAILS_TEMPLATE,
        ITEM_LIST_SEQUENCE_TEMPLATE,
        BROWSE_LIBRARY_TEMPLATE,
        FOLLOW_UP_QUESTION,
        APLA,
        NOT_UNDERSTOOD,
        HELP,
        GENERIC_HEADLINE_TEMPLATE,
        ROOM_SELECTION_TEMPLATE,
        VERTICAL_TEXT_LIST_TEMPLATE
    }

    public class BaseProperties : IProperties
    {
        public string HeadlinePrimaryText { get; set; }
        public RenderDocumentType documentType { get; set; }
        public string url { get; set; }
        public string audioUrl { get; set; }
    }
}

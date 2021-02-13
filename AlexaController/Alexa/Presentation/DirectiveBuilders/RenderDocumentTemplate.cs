using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa.Presentation.DirectiveBuilders
{
    public enum RenderDocumentType
    {
        NONE,
        ITEM_DETAILS_TEMPLATE,
        ITEM_LIST_SEQUENCE_TEMPLATE,
        BROWSE_LIBRARY_TEMPLATE,
        QUESTION_TEMPLATE,
        //VIDEO,
        NOT_UNDERSTOOD,
        HELP,
        GENERIC_HEADLINE_TEMPLATE,
        ROOM_SELECTION_TEMPLATE,
        VERTICAL_TEXT_LIST_TEMPLATE
    }
    
    public class RenderDocumentTemplate 
    {
        public RenderDocumentType renderDocumentType { get; set; }
        public string HeaderTitle                    { get; set; } = "";
        public List<BaseItem> baseItems              { get; set; }
        public string HeadlinePrimaryText            { get; set; } = "";
        public string HeaderAttributionImage         { get; set; }
    }
}

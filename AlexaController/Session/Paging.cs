using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DirectiveBuilders;


namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack                                 { get; set; }
        public Dictionary<int, InternalRenderDocumentQuery> pages { get; set; }
        public int currentPage                                { get; set; }
    }
}
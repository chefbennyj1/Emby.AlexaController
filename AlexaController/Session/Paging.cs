using System.Collections.Generic;
using AlexaController.Alexa;


namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack                                 { get; set; }
        public Dictionary<int, RenderDocumentQuery> pages { get; set; }
        public int currentPage                                { get; set; }
    }
}
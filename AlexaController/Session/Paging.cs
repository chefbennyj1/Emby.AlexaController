using System.Collections.Generic;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;


namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack                                 { get; set; }
        public Dictionary<int, IRenderDocumentTemplate> pages { get; set; }
        public int currentPage                                { get; set; }
    }
}
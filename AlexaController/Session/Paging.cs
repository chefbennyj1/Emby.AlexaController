using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources;


namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack                                 { get; set; }
        public Dictionary<int, IDataSource> pages { get; set; }
        public int currentPage                                { get; set; }
    }
}
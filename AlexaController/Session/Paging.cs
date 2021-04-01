using AlexaController.EmbyAplDataSource.DataSourceProperties;
using System.Collections.Generic;

namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack { get; set; }
        public Dictionary<int, Properties<MediaItem>> pages { get; set; }
        public int currentPage { get; set; }
    }
}
using AlexaController.Alexa.Presentation.DataSources;
using System.Collections.Generic;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;

namespace AlexaController.Session
{
    public class Paging
    {
        public bool canGoBack { get; set; }
        public Dictionary<int, Properties<MediaItem>> pages { get; set; }
        public int currentPage { get; set; }
    }
}
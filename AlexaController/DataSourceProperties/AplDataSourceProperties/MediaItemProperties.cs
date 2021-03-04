using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources;

// ReSharper disable once InconsistentNaming

namespace AlexaController.DataSourceProperties.AplDataSourceProperties
{
    public class MediaItemProperties : IProperties
    {
        public string url                      { get; set; }
        public RenderDocumentType documentType { get; set; }
        public List<SimilarItem> similarItems  { get; set; }
        public MediaItem item                  { get; set; }
        public List<MediaItem> items           { get; set; }
    }
    
}



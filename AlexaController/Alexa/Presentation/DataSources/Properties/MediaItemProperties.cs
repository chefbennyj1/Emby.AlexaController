using System.Collections.Generic;

// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.Presentation.DataSources.Properties
{
    
    public class MediaItemProperties : IProperties
    {
        public string url                      { get; set; }
        public List<SimilarItem> similarItems  { get; set; }
        public MediaItem item                  { get; set; }
        public List<MediaItem> items           { get; set; }
    }

    public class MediaItem
    {
        public string endTime             { get; set; }
        public string runtimeMinutes      { get; set; }
        public string primaryImageSource  { get; set; }
        public string tagLine             { get; set; }
        public string genres              { get; set; }
        public long id                    { get; set; }
        public string name                { get; set; }
        public string index               { get; set; }
        public string premiereDate        { get; set; }
        public string type                { get; set; }
        public string overview            { get; set; }
        public string officialRating      { get; set; }
        public string videoBackdropSource { get; set; }
        public string backdropImageSource { get; set; }
        public string logoImageSource     { get; set; }
        public string videoOverlaySource  { get; set; }
    }

    public class SimilarItem
    {
        public string thumbImageSource { get; set; }
        public long id                 { get; set; }
    }



}

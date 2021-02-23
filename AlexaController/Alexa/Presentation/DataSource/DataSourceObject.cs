using System.Collections.Generic;

// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.Presentation.DataSource
{
    public class DataSourceObject : IDataSource
    {
        public object type => "object";
        public Properties properties { get; set; }
        public string objectID       { get; set; }
        public string description    { get; set; }
        //TODO: Transformers
    }

    public class Properties
    {
        public string url           { get; set; }
        public List<Recommendation> recommendations { get; set; }
        public Item item            { get; set; }
        public List<Item> items     { get; set; }
    }

    public class Item
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

    public class Recommendation
    {
        public string thumbImageSource { get; set; }
        public long id { get; set; }
    }
}

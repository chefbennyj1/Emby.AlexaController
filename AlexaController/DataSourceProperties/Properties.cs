using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;

namespace AlexaController.DataSourceProperties
{
    public class Properties<T> : BaseDataSourceProperties<T> where T :class
    {
        public RenderDocumentType documentType { get; set; }
        public string theme                    { get; set; } = "dark";
        public string url                      { get; set; }
        public string audioUrl                 { get; set; }
        public string text                     { get; set; }
        public string videoUrl                 { get; set; }
        public List<T> similarItems     { get; set; }
        public List<Value> values              { get; set; }
    }
    public class Value : IProperty
    {
        public string value { get; set; }
    }
    
}

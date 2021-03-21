using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.EmbyAplDataSourceManagement.PropertyModels;
using System.Collections.Generic;

namespace AlexaController.EmbyAplDataSourceManagement
{
    public class Properties<T> : BaseDataSourceProperties<T> where T : class
    {
        public RenderDocumentType documentType { get; set; }
        public string theme { get; set; } = "dark";
        public string url { get; set; }
        public string audioUrl { get; set; }
        public string text { get; set; }
        public string videoUrl { get; set; }
        public List<T> similarItems { get; set; }
        public List<Value> values { get; set; }
    }
    public class Value
    {
        public string value { get; set; }
    }

}

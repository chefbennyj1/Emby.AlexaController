using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class DataSource : IDataSource
    {
        public object type { get; }
        public string objectID { get; set; }
        public string description { get; set; }
        public IProperties properties { get; set; }
        public List<ITransformer> transformers { get; set; }
    }
}

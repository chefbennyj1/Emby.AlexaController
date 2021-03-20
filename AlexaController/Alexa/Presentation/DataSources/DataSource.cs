using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.Presentation.DataSources.Transformers;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class DataSource : IDataSource 
    {
        public object type { get; set; }
        // ReSharper disable once InconsistentNaming
        public string objectID { get; set; }
        public string description { get; set; }
        public IProperties properties { get; set; }
        public List<ITransformer> transformers { get; set; }


        //public string listId                   { get; set; }
        //public int startIndex                  { get; set; }
        //public int minimumInclusiveIndex       { get; set; }
        //public int maximumExclusiveIndex       { get; set; }
        //public List<T> items                   { get; set; }



    }
}

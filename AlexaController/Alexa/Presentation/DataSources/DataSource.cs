using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.DataSources.Properties;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public class DataSource : IDataSource
    {
        public object type                     { get; }
        public string objectID                 { get; set; }
        public string description              { get; set; }
        public IProperties properties          { get; set; }
        public List<ITransformer> transformers { get; set; }
    }
}

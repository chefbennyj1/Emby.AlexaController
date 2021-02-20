using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.ResponseData.Model.DataSources
{
    public class DataSourceObject : IDataSource
    {
        public object type => "object";
        public Properties properties { get; set; }
        public string objectID { get; set; }
        public string description { get; set; }
        //TODO: Transformers
    }

    public class Properties
    {
        public object value { get; set; }
        public List<object> values { get; set; }
    }
}

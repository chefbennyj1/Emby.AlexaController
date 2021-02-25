using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.DataSourceModel
{
    public class HelpDataSource : IDataSource
    {
        public object type => "object";
        public string objectID { get; set; }
        public string description { get; set; }
        public IProperties properties { get; set; }
    }

    public class HelpDataSourceProperties : IProperties
    {
        public List<HelpValues> helpContent { get; set; }
    }

    public class HelpValues
    {
        public string value { get; set; }
    }
}

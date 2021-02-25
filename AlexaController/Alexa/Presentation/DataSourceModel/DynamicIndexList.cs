using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.DataSourceModel
{
    public class DynamicIndexList : IDataSource
    {
        public object type => nameof(DynamicIndexList);
        public string objectID           { get; set; }
        public string description        { get; set; }
        public string listId             { get; set; }
        public int startIndex            { get; set; }
        public int minimumInclusiveIndex { get; set; }
        public int maximumExclusiveIndex { get; set; }
        public IProperties properties    { get; set; }
        public List<object> items        { get; set; }
    }

    public class DynamicIndexListProperties : IProperties
    {

    }
}

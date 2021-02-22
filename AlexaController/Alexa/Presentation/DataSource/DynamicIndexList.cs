using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.DataSource
{
    public class DynamicIndexList : IDataSource
    {
        public object type => nameof(DynamicIndexList);
        public string listId { get; set; }
        public int startIndex { get; set; }
        public int minimumInclusiveIndex { get; set; }
        public int maximumExclusiveIndex { get; set; }
        public List<object> items { get; set; }
    }
    
}

using System.Collections.Generic;

namespace AlexaController.Api.RequestData
{
    // ReSharper disable once InconsistentNaming
    public class slotData
    {
        public string name          { get; set; }
        public string value         { get; set; }
        public string canUnderstand { get; set; }
        public string canFulfill    { get; set; }
        public SlotValue slotValue { get; set; }
    }

    public class SlotValue
    {
        public string type { get; set; }
        public string value { get; set; }
        public List<Value> values { get; set; } 
    }

    public class Value
    {
        public string type { get; set; } 
        public string value { get; set; } 
    }
}

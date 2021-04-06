using System.Collections.Generic;

namespace AlexaController.Alexa.RequestModel
{
    // ReSharper disable once InconsistentNaming
    public class slotData
    {
        public string name { get; set; }
        public string value { get; set; }
        public string canUnderstand { get; set; }
        public string canFulfill { get; set; }
        public SlotValue slotValue { get; set; }
    }

    public class SlotValue
    {
        public string type { get; set; }
        public string value { get; set; }
        public List<Value> values { get; set; }
        public Resolutions resolutions { get; set; } 
    }

    public class Value
    {
        public string type { get; set; }
        public string value { get; set; }
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Resolutions
    {
        public List<ResolutionsPerAuthority> resolutionsPerAuthority { get; set; }
    }
    public class ResolutionsPerAuthority
    {
        public string authority { get; set; }
        public Status status { get; set; }
        public List<Values> values { get; set; }
    }
    public class Values {
        public Value value { get; set; } 

    }
    public class Status
    {
        public string code { get; set; }
    }
}

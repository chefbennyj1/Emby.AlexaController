using System;

namespace AlexaController.Api.RequestData
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Intent : Attribute
    {
        public string name { get; set; }
        public string confirmationStatus { get; set; }
        public Slots slots { get; set; }
    }
}

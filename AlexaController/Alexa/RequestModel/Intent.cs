using System;

namespace AlexaController.Alexa.RequestModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Intent : Attribute
    {
        public string name { get; set; }
        public string confirmationStatus { get; set; }
        public Slots slots { get; set; }
    }
}

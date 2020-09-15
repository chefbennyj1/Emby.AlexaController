using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SendEvent : Command
    {
        public object type => nameof(SendEvent);
        public List<object> arguments  { get; set; }
        public List<object> components { get; set; }
    }
}
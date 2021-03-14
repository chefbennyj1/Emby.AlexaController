using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SendEvent : ICommand
    {
        public object type => nameof(SendEvent);

        public List<object> arguments { get; set; }
        public List<object> components { get; set; }
        public bool screenLock { get; set; }
        public int delay { get; set; }
    }
}
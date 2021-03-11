using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class Sequential : ICommand
    {
        public List<ICommand> commands { get; set; }
        public object type => nameof(Sequential);
        public object repeatCount { get; set; }
        public bool screenLock { get; set; }
        public int delay { get; set; }
    }
}
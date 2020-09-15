using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class Sequential : Command
    {
        public List<Command> commands { get; set; }
        public object type => nameof(Sequential);
        public int repeatCount { get; set; }
    }
}
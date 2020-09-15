using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class Parallel : Command
    {
        public List<Command> commands { get; set; }
        public object type => nameof(Parallel);
        
    }
}
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class Parallel : ICommand
    {
        public List<ICommand> commands { get; set; }
        public object type => nameof(Parallel);

        public bool screenLock { get; set; }
        public int delay { get; set; }
        public string when { get; set; }
    }
}
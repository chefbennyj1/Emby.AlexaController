using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class HandleTick
    {
        public int minimumDelay { get; set; }
        public List<ICommand> commands { get; set; }
    }
}

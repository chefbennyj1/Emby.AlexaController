using AlexaController.Alexa.Presentation.APL.Commands;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class HandleTick
    {
        public int minimumDelay { get; set; }
        public List<ICommand> commands { get; set; }
    }
}

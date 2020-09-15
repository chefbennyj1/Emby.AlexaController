using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class AutoPage : Command
    {
        public string type => nameof(AutoPage);
        public int count { get; set; }
        public int duration { get; set; }
        public string componentId { get; set; }
    }
}

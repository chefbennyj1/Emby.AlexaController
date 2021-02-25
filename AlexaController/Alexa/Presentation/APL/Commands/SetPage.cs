using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SetPage : ICommand
    {
        public object type => nameof(SetPage);
        public int value { get; set; }
        public string componentId { get; set; }
    }
}

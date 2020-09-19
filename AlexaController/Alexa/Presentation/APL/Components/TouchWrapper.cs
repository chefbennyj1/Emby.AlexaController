using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class TouchWrapper : Item
    {
        public ICommand onPress { get; set; }
        public object type => nameof(TouchWrapper);

    }
}
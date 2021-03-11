using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Sequencer : AudioBaseComponent
    {
        public object type => nameof(Sequencer);
        public List<AudioBaseComponent> items { get; set; }
        public AudioBaseComponent item { get; set; }
    }
}

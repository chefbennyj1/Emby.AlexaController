using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Sequencer : AudioBaseItem
    {
        public object type => nameof(Sequencer);
        public List<AudioBaseItem> items { get; set; }
        public AudioBaseItem item { get; set; }
    }
}

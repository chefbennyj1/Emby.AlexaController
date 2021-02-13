using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APLA.Components
{
    public class Sequencer : AudioItem
    {
        public object type => nameof(Sequencer);
        public List<AudioItem> items { get; set; }
        public AudioItem item { get; set; }
    }
}

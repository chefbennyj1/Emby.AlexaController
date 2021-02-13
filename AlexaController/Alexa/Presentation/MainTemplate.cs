using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation
{
    public class MainTemplate : IMainTemplate
    {
        public List<string> parameters { get; set; }
        public List<IItem> items { get; set; }
        public IItem item { get; set; }
    }
}

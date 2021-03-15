using System;
using System.Collections.Generic;
using System.Text;
using AlexaController.Alexa.Presentation.APL;

namespace AlexaController.AlexaPresentationManagers.Imports
{
    public static class ImportsFactory
    {
        public static readonly List<Import> Imports = new List<Import>()
        {
            new Import()
            {
                name    = "alexa-layouts",
                version = "1.2.0"
            },
            new Import()
            {
                name    = "alexa-viewport-profiles",
                version = "1.1.0"
            }
        };
    }
}

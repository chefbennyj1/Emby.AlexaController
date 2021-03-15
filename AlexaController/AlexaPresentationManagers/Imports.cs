using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL;

namespace AlexaController.AlexaPresentationManagers
{
    public static class Imports
    {
        public static List<IImport> GetImports => new List<IImport>()
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

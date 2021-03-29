using AlexaController.Alexa.Presentation.APL;
using System.Collections.Generic;

namespace AlexaController.EmbyAplManagement
{
    // ReSharper disable once UnusedType.Global
    public static class ImportsManager
    {
        public static List<IImport> RenderImportsList => new List<IImport>()
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

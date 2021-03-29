using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL
{
    public class Document : IDocument
    {
        public string type => "APL";
        public string version => "1.1";
        public List<IExtension> extensions { get; set; }
        public ISettings settings { get; set; }
        public string theme { get; set; }
        public List<IImport> import { get; set; }
        public List<IResource> resources { get; set; }
        public List<ICommand> onMount { get; set; }
        public IMainTemplate mainTemplate { get; set; }
        public Dictionary<string, IAlexaVectorGraphic> graphics { get; set; }
        public Dictionary<string, ICommand> commands { get; set; }

    }

}

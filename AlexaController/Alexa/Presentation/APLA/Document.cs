using AlexaController.Alexa.Presentation.APL;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APLA
{
    public class Document : IDocument
    {
        public string type => "APLA";
        public string version => "0.91";
        public Settings settings { get; set; }
        public List<IResource> resources { get; set; }
        public IMainTemplate mainTemplate { get; set; }
    }
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.VectorGraphics;

namespace AlexaController.Alexa.Presentation.APL
{
    public class Document : IDocument
    {
        public string type    => "APL";
        public string version => "1.1";
        public Settings settings                               { get; set; }
        public string theme                                    { get; set; }
        public List<Import> import                             { get; set; }
        public List<Resource> resources                        { get; set; }
        public List<ICommand> onMount                          { get; set; }
        public IMainTemplate mainTemplate                      { get; set; }
        public Dictionary<string, AlexaVectorGraphic> graphics { get; set; }
        public Dictionary<string, ICommand> commands           { get; set; }
       
    }
    
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL
{
    public interface IDocument
    {
        string type                          { get; }
        string version                       { get; }
        Settings settings                    { get; }
        string theme                         { get; }
        List<IImport> import                 { get; }
        List<IResource> resources            { get; }
        List<ICommand> onMount               { get; }
        IMainTemplate mainTemplate           { get; }
        Dictionary<string, Graphic> graphics { get; }
    }

    public class Document : IDocument
    {
        public string type => "APL";
        public string version => "1.1";
        public Settings settings                    { get; set; }
        public string theme                         { get; set; }
        public List<IImport> import                 { get; set; }
        public List<IResource> resources            { get; set; }
        public List<ICommand> onMount               { get; set; }
        public IMainTemplate mainTemplate           { get; set; }
        public Dictionary<string, Graphic> graphics { get; set; }
    }
    
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APL;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.ResponseData.Model.DataSources;

namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IDirective
    {
        string type { get; set; }
        string token { get; set; }
        IDocument document { get; set; }
        List<ICommand> commands { get; set; }
        Dictionary<string, IDataSource> datasources { get; set; }
        string speech { get; set; }
    }

    public class Directive : IDirective
    {
        public string type { get; set; }
        public string token { get; set; }
        public IDocument document { get; set; }
        public List<ICommand> commands { get; set; }
        public Dictionary<string, IDataSource> datasources { get; set; }
        

        //Progressive Response element
        public string speech { get; set; }
    }
}
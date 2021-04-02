using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.Directives
{
    public class AplaRenderDocumentDirective : IRenderDocument, IDirective
    {
        public string type => "Alexa.Presentation.APLA.RenderDocument";
        public string token { get; set; }
        public IDocument document { get; set; }
        public List<ICommand> commands { get; set; }
        public DataSource datasources { get; set; }
        public Dictionary<string, IDocument> sources { get; set; }
    }
}

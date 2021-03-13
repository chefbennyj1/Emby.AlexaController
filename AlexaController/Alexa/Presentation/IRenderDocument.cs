using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;

namespace AlexaController.Alexa.Presentation
{
    public interface IRenderDocument 
    {
        string token { get; set; }
        IDocument document { get; set; }
        List<ICommand> commands { get; set; }
        Dictionary<string, IDataSource> datasources { get; set; }
        Dictionary<string, IDocument> sources { get; set; }
    }
}

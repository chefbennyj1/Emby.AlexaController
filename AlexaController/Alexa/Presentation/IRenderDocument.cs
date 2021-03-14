using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.DataSources;
using System.Collections.Generic;

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

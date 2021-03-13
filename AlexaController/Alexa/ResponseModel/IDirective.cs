using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.DataSources;

namespace AlexaController.Alexa.ResponseModel
{
    public interface IDirective
    {
        string type { get; }
        //Dictionary<string, IDataSource<T>> datasources { get; set; }
    }
    
}
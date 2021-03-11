using System.Collections.Generic;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.Presentation.DataSources.Transformers;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public interface IDataSource
    {
        object type { get; }
        string objectID { get; set; }
        string description { get; set; }
        IProperties properties { get; set; }
        List<ITransformer> transformers { get; set; }
    }
}

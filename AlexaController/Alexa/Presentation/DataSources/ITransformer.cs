using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.DataSources
{
    public interface ITransformer
    {
        string inputPath   { get; set; }
        string outputName  { get; set; }
        string transformer { get; set; }
        string template    { get; set; }
    }
}

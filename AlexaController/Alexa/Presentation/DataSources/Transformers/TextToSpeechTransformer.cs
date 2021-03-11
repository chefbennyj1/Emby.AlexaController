using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.DataSources.Transformers
{
    public class TextToSpeechTransformer : ITransformer
    {
        public string inputPath { get; set; }
        public string outputName { get; set; }
        public string transformer => "textToSpeech";
    }
}

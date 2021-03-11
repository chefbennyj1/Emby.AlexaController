namespace AlexaController.Alexa.Presentation.DataSources.Transformers
{
    public class AplaSpeechTransformer :  ITransformer
    {
        public string inputPath { get; set; }
        public string outputName { get; set; }
        public string transformer => "aplAudioToSpeech";
        public string template { get; set; }
    }
}

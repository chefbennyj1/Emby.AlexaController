namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IOutputSpeech
    {
        string type   { get; }
        string ssml   { get; set; }
        string phrase { get; set; }
        string sound  { get; set; }
    }

    public class OutputSpeech : IOutputSpeech
    {
        public string type => "SSML";
        public string ssml                                                 { get; set; }
        public string phrase                                               { get; set; }
        public string sound                                                { get; set; } = string.Empty;

        
    }
}
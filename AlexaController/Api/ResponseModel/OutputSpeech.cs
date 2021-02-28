namespace AlexaController.Api.ResponseModel
{
    public class OutputSpeech 
    {
        public string type => "SSML";
        public string ssml   { get; set; }
        public string phrase { get; set; }
        public string sound  { get; set; } = string.Empty;
    }
}
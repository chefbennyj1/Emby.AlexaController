using AlexaController.Alexa.Model.ResponseData;

namespace AlexaController.Alexa.Presentation.APL
{
    public class Metadata
    {
        public string title { get; set; }
        public string subtitle { get; set; }
    }

    public class VideoItem
    {
        public string source { get; set; }
        public Metadata metadata { get; set; }
    }

    public class VideoApp : Directive
    {
        public new string type => "VideoApp.Launch";
        public VideoItem videoItem { get; set; }
    }
}

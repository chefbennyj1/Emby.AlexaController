namespace AlexaController.Api.RequestData
{
    public class AudioPlayer
    {
        public string playerActivity { get; set; }
        public string token { get; set; }
        public int offsetInMilliseconds { get; set; }
    }
}

namespace AlexaController.Alexa.RequestData.Model
{
    public class AmazonSession
    {
        public bool @new { get; set; }
        public string sessionId { get; set; }
        public Application application { get; set; }
        public Attributes attributes { get; set; }
        public User user { get; set; }
    }
}

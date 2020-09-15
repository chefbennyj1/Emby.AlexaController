namespace AlexaController.Alexa.RequestData.Model
{
    public class System
    {
        public Device device { get; set; }
        public Application application { get; set; }
        public User user { get; set; }
        public Person person { get; set; }
        public string apiEndpoint { get; set; }
        public string apiAccessToken { get; set; }
    }
}

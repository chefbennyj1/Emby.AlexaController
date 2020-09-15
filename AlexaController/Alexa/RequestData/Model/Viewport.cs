namespace AlexaController.Alexa.RequestData.Model
{
    public class Viewport
    {
        public string type                             { get; set; }
        public string id                               { get; set; }
        public string shape                            { get; set; }
        public Configuration configuration             { get; set; }
        public string mode                             { get; set; }
        public int pixelWidth                          { get; set; }
        public int pixelHeight                         { get; set; }
        public int dpi                                 { get; set; }
    }
}
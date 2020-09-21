namespace AlexaController.Alexa.Presentation.APL
{
    public interface IExtensions
    {
        string name { get; set; }
        string uri { get; set; }
    }

    public class Extensions : IExtensions
    {
        public string name { get; set; }
        public string uri { get; set; }
    }
}

namespace AlexaController.Alexa.Presentation.APL
{
    public interface IImport
    {
        string name { get; set; }
        string version { get; set; }
    }

    public class Import : IImport
    {
        public string name { get; set; }
        public string version { get; set; }
    }
}

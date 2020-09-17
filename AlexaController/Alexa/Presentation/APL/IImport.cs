namespace AlexaController.Alexa.Presentation.APL
{
    public interface IImport
    {
        string name    { get; }
        string version { get; }
    }

    public class Import : IImport
    {
        public string name    { get; set; }
        public string version { get; set; }
    }
}

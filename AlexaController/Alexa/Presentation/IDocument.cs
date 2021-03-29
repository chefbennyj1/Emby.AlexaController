namespace AlexaController.Alexa.Presentation
{
    public interface IDocument
    {
        string type { get; }
        string version { get; }
        IMainTemplate mainTemplate { get; }
    }
}
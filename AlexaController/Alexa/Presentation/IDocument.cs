namespace AlexaController.Alexa.Presentation
{
    public interface IDocument
    {
        //TODO there is a background property on Document: https://developer.amazon.com/en-US/docs/alexa/alexa-presentation-language/apl-document.html
        string type { get; }
        string version { get; }
        IMainTemplate mainTemplate { get; }
    }
}
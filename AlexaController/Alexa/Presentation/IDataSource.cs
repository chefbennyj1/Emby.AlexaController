namespace AlexaController.Alexa.Presentation
{
    public interface IDataSource
    {
        object type { get; }
        string objectID { get; set; }
        string description { get; set; }
        IProperties properties { get; set; }
    }

    public interface IProperties
    {

    }
}

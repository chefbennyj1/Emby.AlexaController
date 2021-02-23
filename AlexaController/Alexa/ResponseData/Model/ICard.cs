namespace AlexaController.Alexa.ResponseData.Model
{
    public interface ICard
    {
        string type                                                        { get; set; }
        string title                                                       { get; set; }
        string content                                                     { get; set; }
        string text                                                        { get; set; }
    }

    public class Card : ICard
    {
        public string type                                                 { get; set; }
        public string title                                                { get; set; }
        public string content                                              { get; set; }
        public string text                                                 { get; set; }
    }
}
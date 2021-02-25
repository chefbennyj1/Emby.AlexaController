namespace AlexaController.Alexa.Model.RequestData
{
    public interface IPerson
    {
        string personId { get; set; }
        string accessToken { get; set; }
    }

    public class Person : IPerson
    {
        public string personId { get; set; }
        public string accessToken { get; set; }
    }
}

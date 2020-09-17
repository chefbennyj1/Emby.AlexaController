namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IHeader
    {
        string requestId { get; set; }
    }

    public class Header : IHeader
    {
        public string requestId { get; set; }
    }
}
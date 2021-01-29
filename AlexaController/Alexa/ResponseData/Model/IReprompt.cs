namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IReprompt {
        IOutputSpeech outputSpeech { get; set; }
    }

    public class Reprompt : IReprompt
    {
        public IOutputSpeech outputSpeech { get; set; } 
    }
}

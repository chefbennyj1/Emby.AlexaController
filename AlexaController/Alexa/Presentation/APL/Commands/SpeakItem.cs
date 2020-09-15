namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SpeakItem : Command
    {
        public string type => nameof(SpeakItem);
        public string componentId { get; set; }
    }
}

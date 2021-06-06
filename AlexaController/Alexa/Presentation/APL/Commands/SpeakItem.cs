namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SpeakItem : ICommand
    {
        public string type => nameof(SpeakItem);
        public string componentId { get; set; }
        public string when { get; set; }
    }
}

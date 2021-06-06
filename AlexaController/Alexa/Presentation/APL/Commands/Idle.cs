namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class Idle : ICommand
    {
        public object type => nameof(Idle);
        public int delay { get; set; }
        public string when { get; set; }
    }
}

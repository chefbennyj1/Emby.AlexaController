namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public interface ICommand
    {
        string when { get; set; }
    }

    public class Command : ICommand
    {
        public object type { get; set; }
        public int duration { get; set; }
        public bool screenLock { get; set; }
        public int delay { get; set; }
        public string when { get; set; }
    }
}

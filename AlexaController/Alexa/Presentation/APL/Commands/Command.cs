namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public interface ICommand
    {
        bool screenLock { get; set; }
        int delay { get; set; }
    }

    public class Command : ICommand
    {
        public bool screenLock { get; set; }
        public int delay { get; set; }
    }
}

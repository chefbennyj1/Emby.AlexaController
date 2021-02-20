namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public interface ICommand
    {
        object type { get; }
        bool screenLock { get; set; }
        int delay { get; set; }
    }

    public class Command : ICommand
    {
        public object type { get; set; }
        public bool screenLock { get; set; }
        public int delay { get; set; }
    }
}

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class AutoPage : ICommand
    {
        public object type => nameof(AutoPage);
        public int count          { get; set; }
        public int duration       { get; set; }
        public string componentId { get; set; }
        public bool screenLock    { get; set; }
        public int delay          { get; set; }
    }
}

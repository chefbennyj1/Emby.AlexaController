namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SetValue : Command
    {
        public object type => nameof(SetValue);

        public string property    { get; set; }
        public object value       { get; set; }
        public string componentId { get; set; }
        public string when        { get; set; }
    }
}

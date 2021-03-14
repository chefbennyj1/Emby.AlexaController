namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class SetPage : ICommand
    {
        public object type => nameof(SetPage);
        public string position { get; set; }
        public object value { get; set; }
        public string componentId { get; set; }
    }
}

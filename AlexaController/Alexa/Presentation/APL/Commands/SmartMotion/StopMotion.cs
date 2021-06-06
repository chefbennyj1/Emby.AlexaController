namespace AlexaController.Alexa.Presentation.APL.Commands.SmartMotion
{
    public class StopMotion : ICommand
    {
        public object type => "SmartMotion:StopMotion";
        public string when { get; set; }
    }
}

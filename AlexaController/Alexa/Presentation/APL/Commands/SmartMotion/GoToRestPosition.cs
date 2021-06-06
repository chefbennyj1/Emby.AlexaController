namespace AlexaController.Alexa.Presentation.APL.Commands.SmartMotion
{
    public class GoToRestPosition : ICommand
    {
        public object type => "SmartMotion:GoToRestPosition";
        public string when { get; set; }
    }
}

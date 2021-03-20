using AlexaController.Alexa.ResponseModel;

namespace AlexaController.Alexa.Presentation.Directives
{
    public class VoiceSpeakDirective : IDirective
    {
        public string type => "VoicePlayer.Speak";
        public string speech { get; set; }
    }
}

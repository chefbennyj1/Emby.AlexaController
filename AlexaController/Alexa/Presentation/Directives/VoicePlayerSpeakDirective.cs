using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.Directives
{
    public class VoiceSpeakDirective : IDirective
    {
        public string type => "VoicePlayer.Speak";
        public string token { get; set; }

        public Dictionary<string, IDataSource> datasources { get; set; }
        //public Dictionary<string, IDocument> sources { get; set; }
        public string speech { get; set; }
    }
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL;

namespace AlexaController.Alexa.ResponseData.Model
{
    public class Directive
    {
        public string type                                                 { get; set; }
        public string token                                                { get; set; }
        public Document document                                           { get; set; }
        public List<object> commands                                       { get; set; }

        //Progressive Response element
        public string speech                                               { get; set; }
    }
}
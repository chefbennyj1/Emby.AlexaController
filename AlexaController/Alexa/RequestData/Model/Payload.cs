using System.Collections.Generic;

namespace AlexaController.Alexa.RequestData.Model
{
    public class Payload
    {
        public string presentationToken                { get; set; }
        public IList<string> arguments                 { get; set; }
        public Source source                           { get; set; }
        public string dialogRequestId                  { get; set; }
    }
}
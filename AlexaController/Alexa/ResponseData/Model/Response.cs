using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;

namespace AlexaController.Alexa.ResponseData.Model
{
    public class Response 
    {
        public Person person                                               { get; set; }
        public OutputSpeech outputSpeech                                   { get; set; }
        public Card card                                                   { get; set; } = null;
        public object shouldEndSession                                     { get; set; }
        public List<Directive> directives                                  { get; set; }

        // Progressive Response elements
        public Header header                                               { get; set; }
        public Directive directive                                         { get; set; }
    }
}
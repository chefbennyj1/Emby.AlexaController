using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;

namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IResponse
    {
        Person person { get; set; }
        OutputSpeech outputSpeech { get; set; }
        Card card { get; set; }
        object shouldEndSession { get; set; }
        List<IDirective> directives { get; set; }
        Header header { get; set; }
        Directive directive { get; set; }
    }

    public class Response : IResponse
    {
        public Person person                                               { get; set; }
        public OutputSpeech outputSpeech                                   { get; set; }
        public Card card                                                   { get; set; } = null;
        public object shouldEndSession                                     { get; set; }
        public List<IDirective> directives                                  { get; set; }

        // Progressive Response elements
        public Header header                                               { get; set; }
        public Directive directive                                         { get; set; }
    }
}
using System.Collections.Generic;

namespace AlexaController.Alexa.Model.ResponseData
{
    public interface IResponse 
    {
        //IPerson person              { get; }
        bool SpeakUserName           { get; set; }
        OutputSpeech outputSpeech   { get; }
        Reprompt reprompt           { get; set; } 
        Card card                   { get; set; }
        object shouldEndSession      { get; }
        List<IDirective> directives  { get; set; }
        Header header               { get; }
        IDirective directive         { get; }
    }

    public class Response : IResponse
    {
        //public IPerson person              { get; set; }
        public OutputSpeech outputSpeech   { get; set; }
        public Reprompt reprompt           { get; set; } 
        public Card card                   { get; set; } = null;
        public object shouldEndSession      { get; set; }
        public List<IDirective> directives  { get; set; }
        public bool SpeakUserName           { get; set; }
        // Progressive Response elements
        public Header header               { get; set; }
        public IDirective directive         { get; set; }
       
    }
}
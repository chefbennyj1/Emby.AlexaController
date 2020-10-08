using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;

namespace AlexaController.Alexa.ResponseData.Model
{
    public interface IResponse 
    {
        //IPerson person              { get; }
        IOutputSpeech outputSpeech  { get; }
        IReprompt reprompt { get; set; } 
        ICard card                   { get; set; }
        object shouldEndSession     { get; }
        List<IDirective> directives { get; set; }
        IHeader header               { get; }
        IDirective directive        { get; }
    }

    public class Response : IResponse
    {
        //public IPerson person              { get; set; }
        public IOutputSpeech outputSpeech  { get; set; }
        public IReprompt reprompt { get; set; } 
        public ICard card                   { get; set; } = null;
        public object shouldEndSession     { get; set; }
        public List<IDirective> directives { get; set; }

        // Progressive Response elements
        public IHeader header               { get; set; }
        public IDirective directive        { get; set; }
    }
}
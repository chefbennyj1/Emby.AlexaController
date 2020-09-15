using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class TransitionValue : Value
    {
        public new string property => "transform";
    }

    public class OpacityValue : Value
    {
        public int from { get; set; }
        public int to   { get; set; }
        public string property => "opacity";
    }

    public class From
    {
        public string translateY  { get; set; }
        public string translateX  { get; set; }
        public string perspective { get; set; }
        public int? rotate        { get; set; }
        public double scaleX { get; set; } = 1;
        public double scaleY { get; set; } = 1;
        public double skewX       { get; set; }
        public double skewY       { get; set; }
    }

    public class To
    {
        public string translateY  { get; set; }
        public string translateX  { get; set; }
        public string perspective { get; set; }
        public int? rotate        { get; set; }
        public double scaleX { get; set; } = 1;
        public double scaleY { get; set; } = 1;
        public double skewX       { get; set; } 
        public double skewY       { get; set; }
    }

    public class Value
    {
        public string type { get; set; }
        public string property { get; set; }
        public List<From> from { get; set; }
        public List<To> to { get; set; }
    }

    public class AnimateItem : Command
    {
        public object type => nameof(AnimateItem);
        public string easing { get; set; }
        public double duration { get; set; }
        public string componentId { get; set; }
        public List<Value> value { get; set; }
        
    }
}
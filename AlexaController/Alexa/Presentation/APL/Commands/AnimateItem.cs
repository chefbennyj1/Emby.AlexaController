using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Commands
{
    public class TransitionValue : IValue
    {
        public List<From> from { get; set; }
        public List<To> to { get; set; }
        public string property => "transform";
        public string type { get; set; }
    }

    public class OpacityValue : IValue
    {
        public int from { get; set; }
        public int to { get; set; }
        public string property => "opacity";
        public string type { get; set; }
    }

    public class From
    {
        public string translateY { get; set; }
        public string translateX { get; set; }
        public string perspective { get; set; }
        public int? rotate { get; set; }
        public double scaleX { get; set; } = 1;
        public double scaleY { get; set; } = 1;
        public double skewX { get; set; }
        public double skewY { get; set; }
    }

    public class To
    {
        public string translateY { get; set; }
        public string translateX { get; set; }
        public string perspective { get; set; }
        public int? rotate { get; set; }
        public double scaleX { get; set; } = 1;
        public double scaleY { get; set; } = 1;
        public double skewX { get; set; }
        public double skewY { get; set; }
    }

    public interface IValue
    {

    }

    public class Value : IValue
    {


    }

    public class AnimateItem : ICommand
    {
        public object type => nameof(AnimateItem);
        public string easing { get; set; }
        public double? duration { get; set; }
        public string componentId { get; set; }
        public List<IValue> value { get; set; }

        public bool screenLock { get; set; }
        public int? delay { get; set; }
    }
}
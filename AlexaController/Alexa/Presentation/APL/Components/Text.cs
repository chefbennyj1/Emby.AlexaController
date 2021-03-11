using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Text : VisualBaseComponent
    {
        public string textAlign { get; set; }
        public string textAlignVertical { get; set; }
        public string fontSize  { get; set; }
        public string fontWeight { get; set; }
        public string fontFamily { get; set; }
        public string text      { get; set; }
        public string shadowRadius { get; set; }
        public string shadowVerticalOffset { get; set; }
        public string shadowColor { get; set; }
        public object type => nameof(Text);

    }
}
using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Text : Item
    {
        public string textAlign { get; set; }
        public string textAlignVertical { get; set; }
        public string fontSize  { get; set; }
        public string text      { get; set; }
        public object type => nameof(Text);

    }
}
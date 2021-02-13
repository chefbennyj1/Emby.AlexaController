using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaHeader : VisualItem
    {
        public bool headerBackButton                     { get; set; }
        public string headerTitle                        { get; set; }
        public string headerAttributionImage             { get; set; }
        public string headerSubtitle                     { get; set; }
        public string headerBackButtonAccessibilityLabel { get; set; }
        public bool headerDivider                        { get; set; }
        public object type => nameof(AlexaHeader);

    }
}
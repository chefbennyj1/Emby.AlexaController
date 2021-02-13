using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaFooter : VisualItem
    {
        public string hintText { get; set; }
        public object type => nameof(AlexaFooter);


    }
}
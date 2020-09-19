using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaFooter : Item
    {
        public string hintText { get; set; }
        public object type => nameof(AlexaFooter);


    }
}
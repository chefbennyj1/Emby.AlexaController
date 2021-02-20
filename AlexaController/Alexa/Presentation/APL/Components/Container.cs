using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Container : VisualItem
    {
        public object type => nameof(Container);
        public string alignItems { get; set; }
        public string justifyContent { get; set; }
        /// <summary>
        /// column (default), row
        /// </summary>
        public string direction  { get; set; }
        /// <summary>
        /// wrap, noWrap (default), wrapReverse
        /// </summary>
        public string wrap { get; set; }
        
    }
}
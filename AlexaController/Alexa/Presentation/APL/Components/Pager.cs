using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Pager : VisualBaseItem
    {
        public string type => nameof(Pager);
        /// <summary>
        /// forwards-only, normal, none, wrap
        /// </summary>
        public string navigation          { get; set; }
        public List<object> onPageChanged { get; set; }
        public int initialPage            { get; set; }
        public List<object> firstItem     { get; set; }
        public List<object> lastItem      { get; set; }
       
    }
    
}

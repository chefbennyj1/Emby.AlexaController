using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Pager : Item
    {
        public string type => nameof(Pager);
        public string navigation { get; set; }
        public List<object> onPageChanged { get; set; }
        public int initialPage { get; set; }
        public List<object> firstItem { get; set; }
        public List<object> lastItem { get; set; }
       
    }
}

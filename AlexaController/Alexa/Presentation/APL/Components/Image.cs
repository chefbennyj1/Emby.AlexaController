using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Image : Item
    {
        public object type => nameof(Image);
        public string source                   { get; set; }
        public string overlayColor             { get; set; }
        public string scale                    { get; set; }
        public OverlayGradient overlayGradient { get; set; }
        
       
    }
}
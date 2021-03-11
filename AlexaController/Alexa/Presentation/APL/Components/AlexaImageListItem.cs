using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaImageListItem : VisualBaseComponent
    {
        public object type => nameof(AlexaImageListItem);
        public string defaultImageSource      { get; set; }
        public bool hideOrdinal               { get; set; }
        public string imageAlignment          { get; set; }
        public string imageAspectRatio        { get; set; }
        public bool imageBlurredBackground    { get; set; }
        public bool imageHideScrim            { get; set; }
        public bool imageMetadataPrimacy      { get; set; }
        public int imageProgressBarPercentage { get; set; }
        public bool imageRoundedCorner        { get; set; }
        public string imageScale              { get; set; }
        public string imageSource             { get; set; }
        public object primaryAction           { get; set; }
        public string primaryText             { get; set; }
        public string providerText            { get; set; }
        public string secondaryText           { get; set; }
        public string tertiaryText            { get; set; }

        
    }
}

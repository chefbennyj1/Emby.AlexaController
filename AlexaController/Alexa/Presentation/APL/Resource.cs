namespace AlexaController.Alexa.Presentation.APL
{
   
    public class Resource 
    {
        public string description    { get; set; }
        public Colors colors         { get; set; }
        public string when           { get; set; }
        public Dimensions dimensions { get; set; }
    }

    public class Colors
    {
        public string colorTextPrimary { get; set; }
    }

    public class Dimensions
    {
        public int textSizeBody          { get; set; }
        public int textSizePrimary       { get; set; }
        public int textSizeSecondary     { get; set; }
        public int textSizeSecondaryHint { get; set; }
        public int? spacingThin          { get; set; }
        public int? spacingSmall         { get; set; }
        public int? spacingMedium        { get; set; }
        public int? spacingLarge         { get; set; }
        public int? spacingExtraLarge    { get; set; }
        public int? marginTop            { get; set; }
        public int? marginLeft           { get; set; }
        public int? marginRight          { get; set; }
        public int? marginBottom         { get; set; }
    }

    public class Settings
    {
        public int idleTimeout { get; set; }
        
    }

    


}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL
{
    
    public class Graphic
    {
        public string type    => "AVG";
        public string version => "1.0";
        public int height                  { get; set; }
        public int width                   { get; set; }
        public int viewportHeight          { get; set; }
        public int viewportWidth           { get; set; }
        public List<Parameter> parameters  { get; set; }
        public List<Item> items            { get; set; }
    }

    public class Parameter
    {
        public string name     { get; set; }
        public string type     { get; set; }
        public object @default { get; set; }
    }
    
    public class Import
    {
        public string name    { get; set; }
        public string version { get; set; }
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

    public class Resource
    {
        public string description    { get; set; }
        public Colors colors         { get; set; }
        public string when           { get; set; }
        public Dimensions dimensions { get; set; }
    }
    
    public class OverlayGradient
    {
        public object type => GetType().Name;
        public List<string> colorRange { get; set; }
        public List<double> inputRange { get; set; }
    }

    public class Settings
    {
        public int idleTimeout { get; set; }
    }

    public class MainTemplate
    {
        public List<string> parameters { get; set; }
        public List<Item> items        { get; set; }
    }
    
    public class Document 
    {
        public string type    => "APL";
        public string version => "1.1";
        public Settings settings                    { get; set; }
        public string theme                         { get; set; }
        public List<Import> import                  { get; set; }
        public List<Resource> resources             { get; set; }
        public List<object> onMount                 { get; set; }
        public MainTemplate mainTemplate            { get; set; }
        public Dictionary<string, Graphic> graphics { get; set; }

    }
}

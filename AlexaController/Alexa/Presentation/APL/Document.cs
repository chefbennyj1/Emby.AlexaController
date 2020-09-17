using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APL.Commands;
using AlexaController.Alexa.Presentation.APL.Components;

namespace AlexaController.Alexa.Presentation.APL
{

    public interface IDocument
    {
        string type                          { get; }
        string version                       { get; }
        Settings settings                    { get; set; }
        string theme                         { get; set; }
        List<Import> import                  { get; set; }
        List<Resource> resources             { get; set; }
        List<ICommand> onMount               { get; set; }
        MainTemplate mainTemplate            { get; set; }
        Dictionary<string, Graphic> graphics { get; set; }
    }

    public class Document : IDocument
    {
        public string type => "APL";
        public string version => "1.1";
        public Settings settings                    { get; set; }
        public string theme                         { get; set; }
        public List<Import> import                  { get; set; }
        public List<Resource> resources             { get; set; }
        public List<ICommand> onMount               { get; set; }
        public MainTemplate mainTemplate            { get; set; }
        public Dictionary<string, Graphic> graphics { get; set; }
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
    
    

    public class Settings
    {
        public int idleTimeout { get; set; }
    }

    public class MainTemplate
    {
        public List<string> parameters { get; set; }
        public List<IItem> items       { get; set; }
    }

    
}

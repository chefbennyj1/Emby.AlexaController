using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public interface IItem
    {
        string style          { get; set; }
        string color          { get; set; }
        string spacing        { get; set; }
        string paddingTop     { get; set; }
        string align          { get; set; }
        string when           { get; set; }
        List<IItem> items     { get; set; }
        IItem item            { get; set; }
        string width          { get; set; }
        string height         { get; set; }
        string position       { get; set; }
        string paddingBottom  { get; set; }
        string paddingLeft    { get; set; }
        string paddingRight   { get; set; }
        int? grow             { get; set; }
        int? shrink           { get; set; }
        string left           { get; set; }
        string right          { get; set; }
        string top            { get; set; }
        string bottom         { get; set; }
        string id             { get; set; }
        double opacity        { get; set; } 
        bool disabled         { get; set; }
        string speech         { get; set; }
        string display        { get; set; }
        string content        { get; set; }
        HandleTick handleTick { get; set; }
        string data { get; set; }
    }
}
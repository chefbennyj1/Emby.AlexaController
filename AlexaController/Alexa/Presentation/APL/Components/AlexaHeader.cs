using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class AlexaHeader : IItem
    {
        public bool headerBackButton                     { get; set; }
        public string headerTitle                        { get; set; }
        public string headerAttributionImage             { get; set; }
        public string headerSubtitle                     { get; set; }
        public string headerBackButtonAccessibilityLabel { get; set; }
        public bool headerDivider                        { get; set; }
        public object type => nameof(AlexaHeader);

        public string style          { get; set; }
        public string color          { get; set; }
        public string spacing        { get; set; }
        public string paddingTop     { get; set; }
        public string align          { get; set; }
        public string when           { get; set; }
        public List<IItem> items     { get; set; }
        public IItem item            { get; set; }
        public string width          { get; set; }
        public string height         { get; set; }
        public string position       { get; set; }
        public string paddingBottom  { get; set; }
        public string paddingLeft    { get; set; }
        public string paddingRight   { get; set; }
        public int? grow             { get; set; }
        public int? shrink           { get; set; }
        public string left           { get; set; }
        public string right          { get; set; }
        public string top            { get; set; }
        public string bottom         { get; set; }
        public string id             { get; set; }
        public double opacity        { get; set; } = 1;
        public bool disabled         { get; set; }
        public string speech         { get; set; }
        public string display        { get; set; }
        public string content        { get; set; }
        public HandleTick handleTick { get; set; }
    }
}
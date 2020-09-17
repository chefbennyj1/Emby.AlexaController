using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class VectorGraphic : IItem
    {
        public object type => nameof(VectorGraphic);
        public string source { get; set; }
        public string scale { get; set; }

        public string style { get; set; }
        public string color { get; set; }
        public string spacing { get; set; }
        public string paddingTop { get; set; }
        public string align { get; set; }
        public string when { get; set; }
        public List<IItem> items { get; set; }
        public IItem item { get; set; }
        public string width { get; set; }
        public string height { get; set; }
        public string position { get; set; }
        public string paddingBottom { get; set; }
        public string paddingLeft { get; set; }
        public string paddingRight { get; set; }
        public int? grow { get; set; }
        public int? shrink { get; set; }
        public string left { get; set; }
        public string right { get; set; }
        public string top { get; set; }
        public string bottom { get; set; }
        public string id { get; set; }
        public double opacity { get; set; } = 1;
        public bool disabled { get; set; }
        public string speech { get; set; }
        public string display { get; set; }
        public string content { get; set; }
    }
}
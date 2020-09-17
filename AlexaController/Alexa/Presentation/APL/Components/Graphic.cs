using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Graphic
    {
        public string type => "AVG";
        public string version => "1.0";
        public int height { get; set; }
        public int width { get; set; }
        public int viewportHeight { get; set; }
        public int viewportWidth { get; set; }
        public List<Parameter> parameters { get; set; }
        public List<IItem> items { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public string type { get; set; }
        public object @default { get; set; }
    }
}

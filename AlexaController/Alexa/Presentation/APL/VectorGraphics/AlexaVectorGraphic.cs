using System.Collections.Generic;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class AlexaVectorGraphic
    {
        public string type => "AVG";
        public string version => "1.0";
        public int height                 { get; set; }
        public int width                  { get; set; }
        public int viewportHeight         { get; set; }
        public int viewportWidth          { get; set; }
        public List<string> parameters    { get; set; }
        public List<IVectorGraphic> items { get; set; }
    }
}

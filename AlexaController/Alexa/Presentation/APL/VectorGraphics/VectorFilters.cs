using System;
using System.Collections.Generic;
using System.Text;

namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class VectorFilters
    {
        public VectorFilterType type { get; set; }
        public int horizontalOffset { get; set; }
        public int verticalOffset { get; set; }
        public int radius { get; set; }
        public string color { get; set; }

    }

    public enum VectorFilterType
    {
        DropShadow
    }
}

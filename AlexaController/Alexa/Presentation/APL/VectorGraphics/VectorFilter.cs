namespace AlexaController.Alexa.Presentation.APL.VectorGraphics
{
    public class VectorFilter
    {
        public VectorFilterType type { get; set; }
        public double horizontalOffset { get; set; }
        public double verticalOffset { get; set; }
        public int radius { get; set; }
        public string color { get; set; }

    }

    public enum VectorFilterType
    {
        DropShadow
    }
}

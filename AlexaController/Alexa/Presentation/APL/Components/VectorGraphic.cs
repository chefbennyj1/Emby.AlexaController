namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class VectorGraphic : Item
    {
        public object type => nameof(VectorGraphic);
        public string source { get; set; }
        public string scale { get; set; }
    }
}
namespace AlexaController.Alexa.Presentation.APL.Components
{
    public class Container : Item
    {
        public object type => nameof(Container);
        public string alignItems { get; set; }
        public string direction  { get; set; }
    }
}
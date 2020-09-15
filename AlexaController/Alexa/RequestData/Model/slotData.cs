namespace AlexaController.Alexa.RequestData.Model
{
    public class slotData
    {
        public string name { get; set; }
        public string value { get; set; }
        public string canUnderstand => "YES";
        public string canFulfill => "YES";
    }
}

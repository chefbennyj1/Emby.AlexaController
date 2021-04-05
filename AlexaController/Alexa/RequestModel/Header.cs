namespace AlexaController.Alexa.RequestModel
{
    public class Header
    {
        /// <summary>
        /// Smart motion namespace -> "namespace": "Alexa.SmartMotion",
        /// </summary>
        
        public string @namespace { get; set; }
        /// <summary>
        /// SmartMotion -> "name": "ModeChanged",
        /// </summary>
        public string name { get; set; }
        public string messageId { get; set; }
        public string dialogRequestId { get; set; }
    }
}

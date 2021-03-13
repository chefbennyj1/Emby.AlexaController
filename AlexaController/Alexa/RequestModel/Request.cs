using System;
using System.Collections.Generic;

namespace AlexaController.Alexa.RequestModel
{
    public class Request 
    {
        public string type { get; set; }
        public string requestId { get; set; }
        public DateTime timestamp { get; set; }
        public string locale { get; set; }
        public Intent intent { get; set; }
        public string presentationToken { get; set; }
        public List<string> arguments { get; set; }
        public Source source { get; set; }
        public string dialogRequestId { get; set; }

        public string token { get; set; }
        public string correlationToken { get; set; }
        public string listId { get; set; }
        public int startIndex { get; set; }
        public int count { get; set; }

    }
}

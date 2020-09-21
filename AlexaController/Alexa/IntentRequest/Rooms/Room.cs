using System.Collections.Generic;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    public class Room
    {
        public string Device      { get; set; }
        public string Name        { get; set; }
        public List<string> Echos { get; set; }
    }
}
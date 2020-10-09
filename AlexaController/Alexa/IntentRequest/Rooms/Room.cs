using System.Collections.Generic;

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    public class Room
    {
        public string DeviceName  { get; set; }
        public string AppName     { get; set; }
        public string AppSvg      { get; set; }
        public string Name        { get; set; }
        public List<string> Echos { get; set; }
    }
}
using System.Collections.Generic;
using AlexaController.Alexa.IntentRequest.Rooms;
using MediaBrowser.Model.Plugins;

// ReSharper disable once CollectionNeverUpdated.Global

namespace AlexaController.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public List<Room> Rooms                           { get; set; }
        public List<UserCorrelation> UserCorrelations     { get; set; }
        public bool EnableParentalControlVoiceRecognition { get; set; }
        public bool EnableServerActivityLogNotifications  { get; set; }
    }

    public class UserCorrelation
    {
        public string AlexaPersonId { get; set; }
        public string EmbyUserId    { get; set; }
        public string EmbyUserName  { get; set; }
    }
}

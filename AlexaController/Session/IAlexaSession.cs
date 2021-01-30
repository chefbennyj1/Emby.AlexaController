using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Api;
using MediaBrowser.Controller.Entities;
using IPerson = AlexaController.Alexa.RequestData.Model.IPerson;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{

    public enum AlexaSessionDisplayType
    {
        NONE,
        ALEXA_PRESENTATION_LANGUAGE,
        VIDEO
    }

    public interface IAlexaSession
    {
        User User                                         { get; set; }
        string SessionId                                  { get; set; }
        string EchoDeviceId                               { get; set; }
        AlexaSessionDisplayType alexaSessionDisplayType   { get; set; }
        IPerson person                                    { get; set; }
        IAlexaRequest PersistedRequestContextData                { get; set; }
        BaseItem NowViewingBaseItem                       { get; set; }
        bool PlaybackStarted                              { get; set; }
        Room room                                         { get; set; }
        Paging paging                                     { get; set; }
    }

    public class AlexaSession : IAlexaSession
    {
        public User User                                         { get; set; }
        public string SessionId                                  { get; set; }
        public string EchoDeviceId                               { get; set; }
        public AlexaSessionDisplayType alexaSessionDisplayType   { get; set; } 
        public IPerson person                                    { get; set; }
        public IAlexaRequest PersistedRequestContextData                { get; set; }
        public BaseItem NowViewingBaseItem                       { get; set; }
        public bool PlaybackStarted                              { get; set; }
        public Room room                                         { get; set; } 
        public Paging paging                                     { get; set; }
    }
}
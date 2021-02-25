using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Viewport;
using AlexaController.Api;
using MediaBrowser.Controller.Entities;
using IPerson = AlexaController.Alexa.Model.RequestData.IPerson;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{

    
    public interface IAlexaSession
    {
        User User                                         { get; set; }
        string SessionId                                  { get; set; }
        string EchoDeviceId                               { get; set; }
        ViewportProfile viewport   { get; set; }
        bool supportsApl { get; set; }
        IPerson person                                    { get; set; }
        IAlexaRequest PersistedRequestContextData         { get; set; }
        BaseItem NowViewingBaseItem                       { get; set; }
        bool PlaybackStarted                              { get; set; }
        Room room                                         { get; set; }
        Paging paging                                     { get; set; }
    }

    public class AlexaSession : IAlexaSession
    {
        public User User                                                { get; set; }
        public string SessionId                                         { get; set; }
        public string EchoDeviceId                                      { get; set; }
        public ViewportProfile viewport          { get; set; }
        public bool supportsApl { get; set; }
        public IPerson person                                           { get; set; }
        public IAlexaRequest PersistedRequestContextData                { get; set; }
        public BaseItem NowViewingBaseItem                              { get; set; }
        public bool PlaybackStarted                                     { get; set; }
        public Room room                                                { get; set; } 
        public Paging paging                                            { get; set; }
    }
}
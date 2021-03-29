using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.Viewport;
using AlexaController.Api;
using MediaBrowser.Controller.Entities;
using IPerson = AlexaController.Alexa.RequestModel.IPerson;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{
    public interface IAlexaSession
    {
        User User { get; set; }
        string SessionId { get; set; }
        string EmbySessionId { get; set; }
        long PlaybackPositionTicks { get; set; }
        string EchoDeviceId { get; set; }
        Context context { get; set; }
        ViewportProfile viewport { get; set; }
        bool supportsApl { get; set; }
        IPerson person { get; set; }
        IAlexaRequest PersistedRequestData { get; set; }
        BaseItem NowViewingBaseItem { get; set; }
        bool PlaybackStarted { get; set; }
        Room room { get; set; }
        bool hasRoom { get; set; }
        Paging paging { get; set; }
    }

    public class AlexaSession : IAlexaSession
    {
        public User User { get; set; }
        public string SessionId { get; set; }
        public string EmbySessionId { get; set; }
        public long PlaybackPositionTicks { get; set; }
        public string EchoDeviceId { get; set; }
        public Context context { get; set; }
        public ViewportProfile viewport { get; set; }
        public bool supportsApl { get; set; }
        public IPerson person { get; set; }
        public IAlexaRequest PersistedRequestData { get; set; }
        public BaseItem NowViewingBaseItem { get; set; }
        public bool PlaybackStarted { get; set; }
        public Room room { get; set; }
        public bool hasRoom { get; set; }
        public Paging paging { get; set; }
    }
}
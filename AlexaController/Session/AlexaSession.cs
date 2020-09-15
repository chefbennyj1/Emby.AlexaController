using AlexaController.Api;
using AlexaController.Configuration;
using MediaBrowser.Controller.Entities;
using Person = AlexaController.Alexa.RequestData.Model.Person;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{

    public enum AlexaSessionDisplayType
    {
        NONE,
        ALEXA_PRESENTATION_LANGUAGE,
        VIDEO
    }

    public class AlexaSession
    {
        public User User { get; set; }
        public string SessionId { get; set; }
        public string DeviceId { get; set; }
        public AlexaSessionDisplayType alexaSessionDisplayType { get; set; } = AlexaSessionDisplayType.NONE;
        public Person person { get; set; }
        public AlexaRequest PersistedRequestData { get; set; }
        public BaseItem NowViewingBaseItem { get; set; }
        public bool PlaybackStarted { get; set; }
        public Room room { get; set; } 
        public Paging paging { get; set; }
    }
}
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

    public interface IAlexaSession
    {
        User User                                       { get; set; }
        string SessionId                                { get; set; }
        string DeviceId                                 { get; set; }
        AlexaSessionDisplayType alexaSessionDisplayType { get; set; }
        Person person                                   { get; set; }
        AlexaRequest PersistedRequestData               { get; set; }
        BaseItem NowViewingBaseItem                     { get; set; }
        bool PlaybackStarted                            { get; set; }
        Room room                                       { get; set; }
        Paging paging                                   { get; set; }
    }

    public class AlexaSession : IAlexaSession
    {
        public User User                                       { get; set; }
        public string SessionId                                { get; set; }
        public string DeviceId                                 { get; set; }
        public AlexaSessionDisplayType alexaSessionDisplayType { get; set; } = AlexaSessionDisplayType.NONE;
        public Person person                                   { get; set; }
        public AlexaRequest PersistedRequestData               { get; set; }
        public BaseItem NowViewingBaseItem                     { get; set; }
        public bool PlaybackStarted                            { get; set; }
        public Room room                                       { get; set; } 
        public Paging paging                                   { get; set; }
    }
}
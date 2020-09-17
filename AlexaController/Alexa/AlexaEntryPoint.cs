using AlexaController.Alexa.IntentRequest.Rooms;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Serialization;

namespace AlexaController.Alexa
{
    public interface IAlexaEntryPoint
    {
        IJsonSerializer JsonSerializer         { get; set; }
        IHttpClient HttpClient                 { get; set; }
        IUserManager UserManager               { get; set; }
        ISessionManager SessionManager         { get; set; }
        ILibraryManager LibraryManager         { get; set; }
        IRoomContextManager RoomContextManager { get; set; }
        IResponseClient ResponseClient         { get; set; }
    }
    public class AlexaEntryPoint : IServerEntryPoint, IAlexaEntryPoint
    {
        public IJsonSerializer JsonSerializer         { get; set; }
        public IHttpClient HttpClient                 { get; set; }
        public IUserManager UserManager               { get; set; }
        public ISessionManager SessionManager         { get; set; }
        public ILibraryManager LibraryManager         { get; set; }
        public IRoomContextManager RoomContextManager { get; set; }
        public IResponseClient ResponseClient         { get; set; }
        public static AlexaEntryPoint Instance { get; private set; }

        // ReSharper disable once TooManyDependencies
        public AlexaEntryPoint(IJsonSerializer json, IHttpClient http, ISessionManager sessionManager, ILibraryManager libraryManager, IUserManager userManager)
        {
            JsonSerializer     = json;
            UserManager        = userManager;
            SessionManager     = sessionManager;
            LibraryManager     = libraryManager;
            RoomContextManager = new RoomContextManager();
            ResponseClient     = new ResponseClient(json, http);
            Instance           = this;
        }
        public void Dispose()
        {
            
        }

        // ReSharper disable once MethodNameNotMeaningful
        public void Run()
        {
           
        }
    }
}



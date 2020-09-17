using System;
using AlexaController.Alexa.IntentRequest.Rooms;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Serialization;

namespace AlexaController
{
    public interface IAlexaEntryPoint
    {
        ISessionManager SessionManager         { get; }
        ILibraryManager LibraryManager         { get; }
        IRoomContextManager RoomContextManager { get; }
        IResponseClient ResponseClient         { get; }
        ILogger Log { get; }
    }
    public class AlexaEntryPoint : IServerEntryPoint, IAlexaEntryPoint
    {
        public ISessionManager SessionManager         { get; }
        public ILibraryManager LibraryManager         { get; }
        public IRoomContextManager RoomContextManager { get; }
        public IResponseClient ResponseClient         { get; }
        public ILogger Log { get; set; }
        public static IAlexaEntryPoint Instance { get; private set; }

        // ReSharper disable once TooManyDependencies
        public AlexaEntryPoint(IJsonSerializer json, IHttpClient http, ISessionManager sessionManager, ILibraryManager libraryManager, ILogManager logManager)
        {
            SessionManager     = sessionManager;
            LibraryManager     = libraryManager;
            Log = logManager.GetLogger(Plugin.Instance.Name);
            RoomContextManager = Activator.CreateInstance<RoomContextManager>();
            ResponseClient     = (IResponseClient)Activator.CreateInstance(typeof(ResponseClient), json, http);
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



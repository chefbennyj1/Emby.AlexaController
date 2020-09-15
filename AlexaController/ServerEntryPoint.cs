using System.Linq;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;

namespace AlexaController
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        public static ServerEntryPoint Instance { get; private set; }
        public ILogger Log { get; set; }
        
        public ServerEntryPoint(ILogManager log, ISessionManager sessionManager)
        {
            Instance = this;
            
            Log = log.GetLogger(Plugin.Instance.Name);
        }
        public void Dispose()
        {
            
        }

        public void Run()
        {
            
        }

       
    }
}

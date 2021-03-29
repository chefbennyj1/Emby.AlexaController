using AlexaController.Exceptions;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Activity;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Session;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AlexaController
{
    public class ServerController : IServerEntryPoint
    {
        private IUserManager UserManager { get; }
        private ISessionManager SessionManager { get; }
        private IActivityManager ActivityManager { get; set; }
        public ILogger Log { get; }
        public static ServerController Instance { get; private set; }

        // ReSharper disable once TooManyDependencies
        public ServerController(ILogManager logMan, ISessionManager sesMan, IActivityManager activityManager, IUserManager userManager)
        {
            SessionManager = sesMan;
            UserManager = userManager;
            ActivityManager = activityManager;
            Log = logMan.GetLogger(Plugin.Instance.Name);
            Instance = this;
        }

        public async Task SendMessageToPluginConfigurationPage<T>(string name, T data)
        {
            await SessionManager.SendMessageToAdminSessions(name, data, CancellationToken.None);
        }

        // ReSharper disable once TooManyArguments
        public async Task CreateActivityEntry(LogSeverity logSeverity, string name, string overview)
        {
            await Task.Run(() => ActivityManager.Create(new ActivityLogEntry()
            {
                Date = DateTimeOffset.Now,
                Id = new Random().Next(1000, 9999),
                Overview = overview,
                UserId = UserManager.Users.FirstOrDefault(u => u.Policy.IsAdministrator)?.Id.ToString(),
                Name = name,
                Type = "Alert",
                ItemId = "",
                Severity = logSeverity
            }));
        }

        // ReSharper disable once TooManyArguments
        private async Task BrowseHome(string room, User user, string deviceId = null, SessionInfo session = null)
        {
            try
            {
                deviceId = deviceId ?? ServerQuery.Instance.GetDeviceIdFromRoomName(room);
                session = session ?? ServerQuery.Instance.GetSession(deviceId);

                await SessionManager.SendGeneralCommand(null, session.Id, new GeneralCommand()
                {
                    Name = "GoHome",
                    ControllingUserId = user.Id.ToString(),

                }, CancellationToken.None);
            }
            catch (Exception)
            {
                throw new Exception("I was unable to browse to the home page.");
            }
        }

        public async Task BrowseItemAsync(IAlexaSession alexaSession, BaseItem request)
        {
            string deviceId;
            try
            {
                deviceId = ServerQuery.Instance.GetDeviceIdFromRoomName(alexaSession.room.Name);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            var session = ServerQuery.Instance.GetSession(deviceId);
            var type = request.GetType().Name;

            // ReSharper disable once ComplexConditionExpression
            if (!type.Equals("Season") || !type.Equals("Series"))
                await BrowseHome(alexaSession.room.Name, alexaSession.User, deviceId, session);

            try
            {
#pragma warning disable 4014
                SessionManager.SendBrowseCommand(null, session.Id, new BrowseRequest()
#pragma warning restore 4014
                {
                    ItemId = request.Id.ToString(),
                    ItemName = request.Name,
                    ItemType = request.MediaType

                }, CancellationToken.None);
            }
            catch
            {
                throw new BrowseCommandException($"I was unable to browse to {request.Name}.");
            }
        }

        public async Task PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item, long? startPositionTicks = null)
        {
            var deviceId = ServerQuery.Instance.GetDeviceIdFromRoomName(alexaSession.room.Name);

            if (string.IsNullOrEmpty(deviceId))
            {
                throw new DeviceUnavailableException($"{alexaSession.room.Name} device is currently not available.");
            }

            var session = ServerQuery.Instance.GetSession(deviceId);

            if (session is null)
            {
                throw new DeviceUnavailableException($"{alexaSession.room.Name} device is currently not available.");
            }

            // ReSharper disable once TooManyChainedReferences
            long startTicks = 0;
            if (startPositionTicks is null)
            {
                if (item.SupportsPositionTicksResume)
                {
                    startTicks = item.PlaybackPositionTicks;
                }
            }
            else
            {
                startTicks = startPositionTicks.Value;
            }

            try 
            {
                await SessionManager.SendPlayCommand(null, session.Id, new PlayRequest
                {
                    StartPositionTicks = startTicks,
                    PlayCommand = PlayCommand.PlayNow,
                    ItemIds = new[] { item.InternalId },
                    ControllingUserId = alexaSession.User.Id.ToString()

                }, CancellationToken.None);
            }
            catch (Exception)
            {
                throw new PlaybackCommandException($"I had a problem playing {item.Name}.");
            }
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

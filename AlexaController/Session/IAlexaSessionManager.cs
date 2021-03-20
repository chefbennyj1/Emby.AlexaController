using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Viewport;
using AlexaController.Api;
using MediaBrowser.Controller.Session;
using System.Collections.Generic;
using System.Linq;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{
    public interface IAlexaSessionManager
    {
        void EndSession(IAlexaRequest alexaRequest);
        IAlexaSession GetSession(IAlexaRequest alexaRequest, User user = null);
        void UpdateSession(IAlexaSession session, Properties<MediaItem> properties, bool? isBack = null);
        double GetPlaybackProgressTicks(IAlexaSession alexaSession);
    }

    public class AlexaSessionManager : IAlexaSessionManager
    {
        public static IAlexaSessionManager Instance { get; private set; }

        private static readonly List<IAlexaSession> OpenSessions = new List<IAlexaSession>();

        private static Dictionary<string, long> PlaybackPositions = new Dictionary<string, long>();

        private ISessionManager SessionManager { get; }

        public AlexaSessionManager(ISessionManager sessionManager)
        {
            Instance = this;
            SessionManager = sessionManager;
            SessionManager.PlaybackStopped += SessionManager_PlaybackStopped;
            SessionManager.PlaybackProgress += SessionManager_PlaybackProgress;
        }

        private static bool SupportsApl(IAlexaRequest alexaRequest)
        {
            if (alexaRequest.context.Viewports is null) return false;

            var viewportUtility = new ViewportUtility();
            var viewportProfile = viewportUtility.GetViewportProfile(alexaRequest.context.Viewport);
            if (viewportProfile == ViewportProfile.UNKNOWN_VIEWPORT_PROFILE) return false;

            return viewportUtility.ViewportSizeIsLessThen(viewportProfile, ViewportProfile.TV_LANDSCAPE_MEDIUM) &&
                   Equals(alexaRequest.context.Viewports[0].type, "APL");
        }

        private static ViewportProfile GetCurrentViewport(IAlexaRequest alexaRequest)
        {
            var viewportUtility = new ViewportUtility();
            return viewportUtility.GetViewportProfile(alexaRequest.context.Viewport);
        }

        public void EndSession(IAlexaRequest alexaRequest)
        {
            OpenSessions.RemoveAll(s => s.SessionId.Equals(alexaRequest.session.sessionId));
        }

        public IAlexaSession GetSession(IAlexaRequest alexaRequest, User user = null)
        {
            // A UserEvent can only happen in an open session because sessions will always start with voice.
            if (string.Equals(alexaRequest.request.type, "Alexa.Presentation.APL.UserEvent"))
            {
                return OpenSessions.FirstOrDefault(s => s.SessionId == alexaRequest.session.sessionId);
            }

            var context       = alexaRequest.context;
            var system        = context.System;
            var person        = system.person;
            var amazonSession = alexaRequest.session;

            IAlexaRequest persistedRequestData = null;
            IAlexaSession sessionInfo = null;
            //Room room                          = null;

            if (OpenSessions.Exists(s => s.SessionId.Equals(amazonSession.sessionId)))
            {
                sessionInfo = OpenSessions.FirstOrDefault(s => s.SessionId == amazonSession.sessionId);

                persistedRequestData = sessionInfo?.PersistedRequestContextData;
                //room = sessionInfo?.room;

                // ReSharper disable once ComplexConditionExpression
                //if (!(person is null) && !(sessionInfo?.person is null))
                //{
                //    if (string.Equals(sessionInfo.person.personId, person.personId))
                //    {
                //        return sessionInfo; // It is the same person speaking - return the sessionInfo.
                //    }
                //}

                // Remove the session from the "OpenSessions" List, and rebuild the session with the new data
                OpenSessions.RemoveAll(s => s.SessionId.Equals(alexaRequest.session.sessionId));

            }

            // Sync AMAZON session Id with our own.
            sessionInfo = new AlexaSession()
            {
                SessionId = amazonSession.sessionId,
                EchoDeviceId = system.device.deviceId,
                supportsApl = SupportsApl(alexaRequest),
                person = person,
                room = sessionInfo?.room,
                hasRoom = !(sessionInfo?.room is null),
                User = user,
                viewport = GetCurrentViewport(alexaRequest),
                PersistedRequestContextData = persistedRequestData,
                paging = new Paging { pages = new Dictionary<int, Properties<MediaItem>>() }
            };

            OpenSessions.Add(sessionInfo);

            return sessionInfo;
        }

        //TODO: data source object can be null IDataSource dataSource = null in params
        public void UpdateSession(IAlexaSession session, Properties<MediaItem> properties, bool? isBack = null)
        {
            if (!(properties is null))
                session = UpdateSessionPaging(session, properties, isBack);

            OpenSessions.RemoveAll(s => s.SessionId.Equals(session.SessionId));

            OpenSessions.Add(session);

        }

        private static IAlexaSession UpdateSessionPaging(IAlexaSession session, Properties<MediaItem> properties, bool? isBack = null)
        {
            if (isBack == true)
            {
                session.paging.pages.Remove(session.paging.currentPage);
                session.paging.currentPage -= 1;

                if (session.paging.pages.Count <= 1) session.paging.canGoBack = false;

                return session;
            }


            if (session.paging.pages.Count == 0)
            {
                //set the pages dictionary with page 1
                session.paging.currentPage = 1;
                session.paging.pages.Add(session.paging.currentPage, properties);

                return session;
            }

            if (!session.paging.pages.ContainsValue(properties))
            {
                session.paging.currentPage += 1;
                session.paging.canGoBack = true;
                session.paging.pages.Add(session.paging.currentPage, properties);

                return session;
            }

            return session;

        }

        private void SessionManager_PlaybackStopped(object sender, MediaBrowser.Controller.Library.PlaybackStopEventArgs e)
        {
            var deviceName = e.DeviceName;
            var config = Plugin.Instance.Configuration;
            var configRooms = config.Rooms;

            if (!configRooms.Exists(r => r.DeviceName == deviceName)) return;

            var room = configRooms.FirstOrDefault(r => r.DeviceName == deviceName);

            if (!OpenSessions.Exists(session => session.room.Name == room?.Name)) return;

            var sessionToUpdate = OpenSessions.FirstOrDefault(session => session.room.Name == room?.Name);

            try
            {
                PlaybackPositions.Remove(sessionToUpdate.SessionId);
            }
            catch { }

            // ReSharper disable once PossibleNullReferenceException
            sessionToUpdate.PlaybackStarted = false;
            UpdateSession(sessionToUpdate, null);
        }

        private void SessionManager_PlaybackProgress(object sender, MediaBrowser.Controller.Library.PlaybackProgressEventArgs e)
        {
            var deviceName = e.DeviceName;
            var config = Plugin.Instance.Configuration;
            var configRooms = config.Rooms;
            if (!configRooms.Exists(r => r.DeviceName == deviceName)) return;
            var room = configRooms.FirstOrDefault(r => r.DeviceName == deviceName);
            if (!OpenSessions.Exists(session => session.room.Name == room?.Name)) return;
            var sessionToUpdate = OpenSessions.FirstOrDefault(session => session.room.Name == room?.Name);

            if (!PlaybackPositions.ContainsKey(sessionToUpdate.SessionId))
            {
                PlaybackPositions.Add(sessionToUpdate.SessionId, e.PlaybackPositionTicks ?? 0);
                return;
            }

            PlaybackPositions[sessionToUpdate.SessionId] = e.PlaybackPositionTicks ?? 0;
        }

        public double GetPlaybackProgressTicks(IAlexaSession alexaSession)
        {
            var progressPercent = 0.0;
            try
            {
                var d = (int)PlaybackPositions[alexaSession.SessionId] / (int)alexaSession.NowViewingBaseItem.RunTimeTicks;
                progressPercent = (d * 100);
            }
            catch
            {

            }

            return progressPercent;

        }
    }
}
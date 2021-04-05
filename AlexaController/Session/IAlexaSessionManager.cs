using AlexaController.Alexa.Viewport;
using AlexaController.Api;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using MediaBrowser.Controller.Session;
using System.Collections.Generic;
using System.Linq;
using User = MediaBrowser.Controller.Entities.User;

namespace AlexaController.Session
{
    public interface IAlexaSessionManager
    {
        void EndSession(IAlexaRequest alexaRequest);
        IAlexaSession GetSession(IAlexaRequest alexaRequest, User user = null);
        void UpdateSession(IAlexaSession session, Properties<MediaItem> properties, bool? isBack = null);
    }

    public class AlexaSessionManager : IAlexaSessionManager
    {
        public static IAlexaSessionManager Instance { get; private set; }

        private static readonly List<IAlexaSession> OpenSessions = new List<IAlexaSession>();

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

            var context = alexaRequest.context;
            var system = context.System;
            var person = system.person;
            var amazonSession = alexaRequest.session;

            IAlexaRequest persistedRequestData = null;
            IAlexaSession sessionInfo = null;
            //Room room                          = null;

            if (OpenSessions.Exists(s => s.SessionId.Equals(amazonSession.sessionId)))
            {
                sessionInfo = OpenSessions.FirstOrDefault(s => s.SessionId == amazonSession.sessionId);

                persistedRequestData = sessionInfo?.PersistedRequestData;
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
                EmbySessionId = !(sessionInfo?.room is null) ? GetCorrespondingEmbySessionId(sessionInfo) : string.Empty,
                context = context,
                EchoDeviceId = system.device.deviceId,
                NowViewingBaseItem = sessionInfo?.NowViewingBaseItem,
                supportsApl = SupportsApl(alexaRequest),
                //person = person,
                room = sessionInfo?.room,
                hasRoom = !(sessionInfo?.room is null),
                User = user,
                viewport = GetCurrentViewport(alexaRequest),
                PersistedRequestData = persistedRequestData,
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
            try
            {
                ServerController.Instance.Log.Info("SESSION UPDATE: " + session.NowViewingBaseItem.Name);
            }
            catch { }
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

        private string GetCorrespondingEmbySessionId(IAlexaSession sessionInfo)
        {
            //There is an ID
            if (!string.IsNullOrEmpty(sessionInfo.EmbySessionId))
            {
                //Is the ID still an active Emby Session
                if (SessionManager.Sessions.ToList().Exists(s => s.Id == sessionInfo.EmbySessionId))
                {
                    return sessionInfo.EmbySessionId; //ID is still good. TODO:Maybe check the device name is still proper.
                }

                return SessionManager.Sessions.FirstOrDefault(s => s.DeviceName == sessionInfo.room.DeviceName).Id;

            }

            return sessionInfo.room is null ? string.Empty : SessionManager.Sessions.FirstOrDefault(s => s.DeviceName == sessionInfo.room.DeviceName)?.Id;
        }

        private void SessionManager_PlaybackStopped(object sender, MediaBrowser.Controller.Library.PlaybackStopEventArgs e)
        {
            var session = OpenSessions.FirstOrDefault(s => s.EmbySessionId == e.Session.Id);

            if (session is null) return;

            session.PlaybackStarted = false;
            OpenSessions.RemoveAll(s => s.EmbySessionId.Equals(session.EmbySessionId));
            OpenSessions.Add(session);
        }

        private void SessionManager_PlaybackProgress(object sender, MediaBrowser.Controller.Library.PlaybackProgressEventArgs e)
        {

            var session = OpenSessions.FirstOrDefault(s => s.EmbySessionId == e.Session.Id);

            if (session is null) return;

            session.PlaybackPositionTicks = e.PlaybackPositionTicks ?? 0;

            OpenSessions.RemoveAll(s => s.EmbySessionId.Equals(session.EmbySessionId));
            OpenSessions.Add(session);

        }
    }
}
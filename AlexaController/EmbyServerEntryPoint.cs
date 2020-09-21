using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using User = MediaBrowser.Controller.Entities.User;

// ReSharper disable once TooManyDependencies
// ReSharper disable once TooManyArguments
// ReSharper disable TooManyChainedReferences
// ReSharper disable once MethodNameNotMeaningful
// ReSharper disable once ComplexConditionExpression

namespace AlexaController
{
    public interface IEmbyServerEntryPoint
    {
        ILogger Log { get; }
        List<BaseItem> GetBaseItems(BaseItem parent, string[] types, User user);
        IEnumerable<SessionInfo> GetCurrentSessions();
        BaseItem GetNextUpEpisode(Intent intent, User user);
        string GetLibraryId(string name);
        BaseItem GetItemById<T>(T id);
        void SendMessageToPluginConfigurationPage<T>(string name, T data);
        ICollectionInfo GetCollectionItems(User userName, string collectionName);
        List<BaseItem> GetEpisodes(int seasonNumber, BaseItem parent, User user);
        IEnumerable<BaseItem> GetLatestMovies(User user, DateTime duration );
        IEnumerable<BaseItem> GetLatestTv(User user, DateTime duration);
        void BrowseItemAsync(IAlexaSession alexaSession, BaseItem request);
        void PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item);
        BaseItem QuerySpeechResultItem(string searchName, string[] type, User user);
        IDictionary<BaseItem, List<BaseItem>> GetItemsByActor(User user, string actorName);
    }

    public interface ICollectionInfo
    {
        long Id { get; set; }
        List<BaseItem> Items { get; set; }
    }

    public class CollectionInfo : ICollectionInfo
    {
        public long Id              { get; set; }
        public List<BaseItem> Items { get; set; }
    }

    public class EmbyServerEntryPoint : EmbySearchUtility, IServerEntryPoint, IEmbyServerEntryPoint
    {
        private ILibraryManager LibraryManager        { get; }
        private ITVSeriesManager TvSeriesManager      { get; }
        private ISessionManager SessionManager        { get; }
        public ILogger Log                           { get; }
        public static IEmbyServerEntryPoint Instance { get; private set; }

        public EmbyServerEntryPoint(ILogManager logMan, ILibraryManager libMan, ITVSeriesManager tvMan, ISessionManager sesMan) : base(libMan)
        {
            LibraryManager  = libMan;
            TvSeriesManager = tvMan;
            SessionManager  = sesMan;
            Log = logMan.GetLogger(Plugin.Instance.Name);
            Instance        = this;
        }

        public void SendMessageToPluginConfigurationPage<T>(string name, T data)
        {
            SessionManager.SendMessageToAdminSessions(name, data, CancellationToken.None);
        }

        public IEnumerable<SessionInfo> GetCurrentSessions()
        {
            return SessionManager.Sessions;
        }

        public List<BaseItem> GetBaseItems(BaseItem parent, string[] types, User user)
        {
            var result = LibraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                Parent = parent,
                IncludeItemTypes = types,
                Recursive = true
            });
            return result.Items.ToList();
        }

        public List<BaseItem> GetEpisodes(int seasonNumber, BaseItem parent, User user)
        {
            var result = LibraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                Parent = parent,
                IncludeItemTypes = new[] { "Episode" },
                ParentIndexNumber = seasonNumber,
                Recursive = true
            });
            return result.Items.ToList();
        }

        public BaseItem GetItemById<T>(T id)
        {
            return LibraryManager.GetItemById(id.ToString());
        }

        public BaseItem GetNextUpEpisode(Intent intent, User user)
        {
            try
            {
                var series = intent.slots.Series;
                var id     = QuerySpeechResultItem(series.value, new[] { "Series" }, user).InternalId;
                var nextUp = TvSeriesManager.GetNextUp(new NextUpQuery()
                {
                    SeriesId = id,
                    UserId   = user.InternalId
                }, user, new DtoOptions());
                
                return nextUp.Items.FirstOrDefault();
            }
            catch
            {
                return null; //Return null and handle no next up episodes in Alexa Response
            }
        }

        public string GetLibraryId(string name)
        {
            return LibraryManager.GetVirtualFolders().FirstOrDefault(r => r.Name == name)?.ItemId;
        }
        
        public ICollectionInfo GetCollectionItems(User user, string collectionName)
        {
            var result = QuerySpeechResultItem(collectionName, new[] { "collections", "Boxset" }, user);

            var collectionItem = LibraryManager.QueryItems(new InternalItemsQuery()
            {
                ListIds        = new[] { result.InternalId },
                EnableAutoSort = true,
                OrderBy        = new[] { ItemSortBy.PremiereDate }.Select(i => new ValueTuple<string, SortOrder>(i, SortOrder.Ascending)).ToArray(),
            });
            
            return new CollectionInfo(){ Id = result.InternalId, Items = collectionItem.Items.ToList() };

        }

        public IEnumerable<BaseItem> GetLatestMovies(User user, DateTime duration)
        {

            // Duration can be request from the user, but will default to anything add in the last 25 days.
            var results = LibraryManager.GetItemIds(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Movie" },
                User             = user,
                MinDateCreated   = duration,
                Limit            = 20,
                EnableAutoSort   = true,
                OrderBy          = new[] { ItemSortBy.DateCreated }.Select(i => new ValueTuple<string, SortOrder>(i, SortOrder.Descending)).ToArray()
            });

            
            return results.Select(id => LibraryManager.GetItemById(id)).ToList();
        }

        public IEnumerable<BaseItem> GetLatestTv(User user, DateTime duration)
        {
            // Duration can be request from the user, but will default to anything add in the last 25 days.
            var results = LibraryManager.GetItemIds(new InternalItemsQuery
            {
                IncludeItemTypes = new[] { "Episode" },
                User             = user,
                MinDateCreated   = duration,
                IsPlayed         = false,
                EnableAutoSort   = true,
                OrderBy          = new[] { ItemSortBy.DateCreated }.Select(i => new ValueTuple<string, SortOrder>(i, SortOrder.Descending)).ToArray()
            });

            return results.Select(id => LibraryManager.GetItemById(id).Parent.Parent).Distinct().ToList();
        }

        private string GetDeviceIdFromRoomName(string room)
        {
            var config = Plugin.Instance.Configuration;
            if (!config.Rooms.Any())
            {
                throw new DeviceUnavailableException($"{room}'s device is currently unavailable.");
            }

            var device = config.Rooms.FirstOrDefault(r => string.Equals(r.Name, room, StringComparison.CurrentCultureIgnoreCase))?.Device;

            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)?.DeviceId;
            }
            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.Client == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.Client == device)?.DeviceId;
            }

            throw new DeviceUnavailableException($"{room}'s device is currently unavailable.");
        }

        private SessionInfo GetSession(string deviceId)
        {
            return SessionManager.Sessions.FirstOrDefault(i => i.DeviceId == deviceId);
        }

        private async void BrowseHome(string room, User user, string deviceId = null, SessionInfo session = null)
        {
            try
            {
                deviceId = deviceId ?? GetDeviceIdFromRoomName(room);
                session = session ?? GetSession(deviceId);

                //DO NOT AWAIT THIS! ALEXA HATES YOU FOR IT
                await SessionManager.SendGeneralCommand(null, session.Id, new GeneralCommand()
                {
                    Name              = "GoHome",
                    ControllingUserId = user.Id.ToString(),

                }, CancellationToken.None);
            }
            catch (Exception)
            {
                throw new Exception("I was unable to browse to the home page.");
            }
        }

        public void BrowseItemAsync(IAlexaSession alexaSession, BaseItem request)
        {
            var deviceId = GetDeviceIdFromRoomName(alexaSession.room.Name);

            if (string.IsNullOrEmpty(deviceId))
            {
                throw new DeviceUnavailableException($"{alexaSession.room.Name}'s device is currently unavailable." );
            }

            var session = GetSession(deviceId);
            var type = request.GetType().Name;
            

            if (!type.Equals("Season") || !type.Equals("Series"))
                BrowseHome(alexaSession.room.Name, alexaSession.User, deviceId, session);
            Task.Delay(1000).Wait();
            try
            {
                SessionManager.SendBrowseCommand(null, session.Id, new BrowseRequest()
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

        public void PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item)
        {
            var deviceId = GetDeviceIdFromRoomName(alexaSession.room.Name);
            
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new DeviceUnavailableException(SpeechStrings.GetPhrase(SpeechResponseType.DEVICE_UNAVAILABLE,null,null, new []{alexaSession.room.Name}));
            }

            var session  = GetSession(deviceId);

            if (session is null)
            {
                throw new DeviceUnavailableException(SpeechStrings.GetPhrase(SpeechResponseType.DEVICE_UNAVAILABLE, null, null, new[] { alexaSession.room.Name }));
            }
            
            // ReSharper disable once TooManyChainedReferences
            long startTicks = item.SupportsPositionTicksResume ? item.PlaybackPositionTicks : 0;

            try
            {
                SessionManager.SendPlayCommand(null, session.Id, new PlayRequest
                {
                    StartPositionTicks = startTicks,
                    PlayCommand = PlayCommand.PlayNow,
                    ItemIds = new[] {item.InternalId},
                    ControllingUserId = alexaSession.User.Id.ToString()

                }, CancellationToken.None);
            }
            catch (Exception)
            {
                throw new PlaybackCommandException($"I had a problem playing {item.Name}.");
            }
        }

        public IDictionary<BaseItem, List<BaseItem>> GetItemsByActor(User user, string actorName)
        {
            actorName = StringNormalization.ValidateSpeechQueryString(actorName);
            var actorQuery = LibraryManager.GetItemsResult(new InternalItemsQuery()
            {
                IncludeItemTypes = new []{ "Person" },
                SearchTerm = actorName,
                Recursive = true
            });
          
            if (actorQuery.TotalRecordCount <= 0) return null;

            var query = LibraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                IncludeItemTypes = new []{"Series", "Movie"},
                Recursive = true,
                PersonIds = new[] { actorQuery.Items[0].InternalId }
            });

            return new Dictionary<BaseItem, List<BaseItem>>() {{ actorQuery.Items[0], query.Items.ToList() }};

        }

        public void Dispose()
        {
            
        }

        public void Run()
        {
            
        }

        
    }
}
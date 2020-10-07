using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;
using MediaBrowser.Controller;
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


namespace AlexaController
{
    public interface IEmbyServerEntryPoint
    {
        Task<string> GetLocalApiUrlAsync();
        ILogger Log { get; }
        QueryResult<BaseItem> GetItemsResult(BaseItem parent, string[] types, User user);
        IEnumerable<SessionInfo> GetCurrentSessions();
        BaseItem GetNextUpEpisode(string seriesName, User user);
        string GetLibraryId(string name);
        BaseItem GetItemById<T>(T id);
        Task SendMessageToPluginConfigurationPage<T>(string name, T data);
        Dictionary<BaseItem, List<BaseItem>> GetCollectionItems(User userName, string collectionName);
        List<BaseItem> GetEpisodes(int seasonNumber, BaseItem parent, User user);
        IEnumerable<BaseItem> GetLatestMovies(User user, DateTime duration );
        IEnumerable<BaseItem> GetLatestTv(User user, DateTime duration);
        Task<List<BaseItem>> GetUpComingTvAsync(DateTime duration);
        Task BrowseItemAsync(IAlexaSession alexaSession, BaseItem request);
        Task PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item);
        BaseItem QuerySpeechResultItem(string searchName, string[] type, User user);
        IDictionary<BaseItem, List<BaseItem>> GetItemsByActor(User user, string actorName);
        Task GoBack(string room, User user);
    }

   
    // ReSharper disable once ClassTooBig
    public class EmbyServerEntryPoint : EmbySearchUtility, IServerEntryPoint, IEmbyServerEntryPoint
    {
        private IServerApplicationHost Host           { get; }
        private ILibraryManager LibraryManager        { get; }
        private ITVSeriesManager TvSeriesManager      { get; }
        private ISessionManager SessionManager        { get; }
        public ILogger Log                            { get; }
        public static IEmbyServerEntryPoint Instance  { get; private set; }

        // ReSharper disable once TooManyDependencies
        public EmbyServerEntryPoint(ILogManager logMan, ILibraryManager libMan, ITVSeriesManager tvMan, ISessionManager sesMan, IServerApplicationHost host) : base(libMan)
        {
            Host            = host;
            LibraryManager  = libMan;
            TvSeriesManager = tvMan;
            SessionManager  = sesMan;
            Log             = logMan.GetLogger(Plugin.Instance.Name);
            Instance        = this;
        }

        public async Task SendMessageToPluginConfigurationPage<T>(string name, T data)
        {
            await SessionManager.SendMessageToAdminSessions(name, data, CancellationToken.None);
        }

        public IEnumerable<SessionInfo> GetCurrentSessions()
        {
            return SessionManager.Sessions;
        }

        public QueryResult<BaseItem> GetItemsResult(BaseItem parent, string[] types, User user)
        {
            var result = LibraryManager.GetItemsResult(new InternalItemsQuery(user)
            {
                Parent = parent,
                IncludeItemTypes = types,
                Recursive = true
            });
            return result;
        }

        public async Task<string> GetLocalApiUrlAsync()
        {
            return await Host.GetLocalApiUrl(CancellationToken.None);
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

        public BaseItem GetNextUpEpisode(string seriesName, User user)
        {
            try
            {
                var id     = QuerySpeechResultItem(seriesName, new[] { "Series" }, user).InternalId;
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
        
        public Dictionary<BaseItem, List<BaseItem>> GetCollectionItems(User user, string collectionName)
        {
            var result = QuerySpeechResultItem(collectionName, new[] { "collections", "Boxset" }, user);

            var collection = LibraryManager.QueryItems(new InternalItemsQuery()
            {
                ListIds        = new[] { result.InternalId },
                EnableAutoSort = true,
                OrderBy        = new[] { ItemSortBy.PremiereDate }.Select(i => new ValueTuple<string, SortOrder>(i, SortOrder.Ascending)).ToArray(),
            });
            
            return new Dictionary<BaseItem, List<BaseItem>>() { { result, collection.Items.ToList() } };

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

        private string GetDeviceIdFromRoomName(string roomName)
        {
            var config = Plugin.Instance.Configuration;
            if (!config.Rooms.Any())
            {
                throw new Exception("There are no rooms setup in configuration.");
            }

            var room = config.Rooms.FirstOrDefault(r => string.Equals(r.Name, roomName, StringComparison.CurrentCultureIgnoreCase));
            if (room is null)
            {
                throw new Exception("That room doesn't exist in the plugin configuration.");
            }

            var device = room.Device;
            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)?.DeviceId;
            }
            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.Client == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.Client == device)?.DeviceId;
            }

            throw new Exception($"{room}'s device is currently unavailable.");
        }

        private SessionInfo GetSession(string deviceId)
        {
            return SessionManager.Sessions.FirstOrDefault(i => i.DeviceId == deviceId);
        }

        // ReSharper disable once TooManyArguments
        private async Task BrowseHome(string room, User user, string deviceId = null, SessionInfo session = null)
        {
            try
            {
                deviceId = deviceId ?? GetDeviceIdFromRoomName(room);
                session = session ?? GetSession(deviceId);
                
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
        
        public async Task GoBack(string room, User user)
        {
            try
            {
                var deviceId = GetDeviceIdFromRoomName(room);
                var session = GetSession(deviceId);

                await SessionManager.SendGeneralCommand(null, session.Id, new GeneralCommand()
                {
                    Name = "Back",
                    ControllingUserId = user.Id.ToString(),

                }, CancellationToken.None);
            }
            catch 
            {
                
            }
        }

        public async Task BrowseItemAsync(IAlexaSession alexaSession, BaseItem request)
        {
            var deviceId = string.Empty;
            try
            {
                deviceId = GetDeviceIdFromRoomName(alexaSession.room.Name);
            }
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
            
            var session = GetSession(deviceId);
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

        public async Task PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item)
        {
            var deviceId = GetDeviceIdFromRoomName(alexaSession.room.Name);
            
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new DeviceUnavailableException(await SpeechStrings.GetPhrase(new SpeechStringQuery()
                {
                    type = SpeechResponseType.DEVICE_UNAVAILABLE, 
                    args = new []{ alexaSession.room.Name }
                }));
            }

            var session  = GetSession(deviceId);

            if (session is null)
            {
                throw new DeviceUnavailableException(await SpeechStrings.GetPhrase(new SpeechStringQuery()
                {
                    type = SpeechResponseType.DEVICE_UNAVAILABLE, 
                    args = new[] { alexaSession.room.Name }
                }));
            }
            
            // ReSharper disable once TooManyChainedReferences
            long startTicks = item.SupportsPositionTicksResume ? item.PlaybackPositionTicks : 0;

            try
            {
                await SessionManager.SendPlayCommand(null, session.Id, new PlayRequest
                {
                    StartPositionTicks = startTicks,
                    PlayCommand        = PlayCommand.PlayNow,
                    ItemIds            = new[] {item.InternalId},
                    ControllingUserId  = alexaSession.User.Id.ToString()

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

        public async Task<List<BaseItem>> GetUpComingTvAsync(DateTime duration)
        {
            var upComing = LibraryManager.GetItemsResult(new InternalItemsQuery()
            {
                MinPremiereDate  = DateTime.Now.AddDays(-1),
                IncludeItemTypes = new[] {"Episode"},
                MaxPremiereDate  = duration,
                OrderBy          = new[] { ItemSortBy.PremiereDate }.Select(i => new ValueTuple<string, SortOrder>(i, SortOrder.Ascending)).ToArray()
            });

            return await Task.FromResult(upComing.Items.ToList());
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Controller.TV;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using MediaBrowser.Model.Session;
using User = MediaBrowser.Controller.Entities.User;

// ReSharper disable once TooManyDependencies
// ReSharper disable once TooManyArguments
// ReSharper disable TooManyChainedReferences
// ReSharper disable once MethodNameNotMeaningful
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Utils
{
    public interface IEmbyControllerUtility
    {
        BaseItem GetNextUpEpisode(Intent intent, User user);
        string GetLibraryId(string name);
        CollectionInfo GetCollectionItems(User userName, string collectionName);
        IEnumerable<BaseItem> GetLatestMovies(User user, DateTime duration );
        IEnumerable<BaseItem> GetLatestTv(User user, DateTime duration);
        void BrowseItemAsync(string room, User user, BaseItem request);
        void PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item, User user);
        BaseItem QuerySpeechResultItems(string searchName, string[] type, User user);
    }

    public class CollectionInfo
    {
        public long Id              { get; set; }
        public List<BaseItem> Items { get; set; }
    }

    public class EmbyControllerUtility : EmbySearchUtility, IServerEntryPoint, IEmbyControllerUtility
    {
        private ILibraryManager LibraryManager        { get; }
        private ITVSeriesManager TvSeriesManager      { get; }
        private ISessionManager SessionManager        { get; }
        
        public static IEmbyControllerUtility Instance { get; private set; }

        public EmbyControllerUtility(ILibraryManager libMan, ITVSeriesManager tvMan, ISessionManager sesMan) : base(libMan)
        {
            LibraryManager  = libMan;
            TvSeriesManager = tvMan;
            SessionManager  = sesMan;
            Instance        = this;
        }
        
        public BaseItem GetNextUpEpisode(Intent intent, User user)
        {
            try
            {
                var series = intent.slots.Series;
                var id     = QuerySpeechResultItems(series.value, new[] { "Series" }, user).InternalId;
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
        
        public CollectionInfo GetCollectionItems(User user, string collectionName)
        {
            var result = QuerySpeechResultItems(collectionName, new[] { "collections", "Boxset" }, user);

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
            if (!config.Rooms.Any()) return string.Empty;

            var device = config.Rooms.FirstOrDefault(r => string.Equals(r.Name, room, StringComparison.CurrentCultureIgnoreCase))?.Device;

            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.DeviceName == device)?.DeviceId;
            }
            if (!ReferenceEquals(null, SessionManager.Sessions.FirstOrDefault(d => d.Client == device)))
            {
                return SessionManager.Sessions.FirstOrDefault(d => d.Client == device)?.DeviceId;
            }

            return string.Empty;
        }

        private SessionInfo GetSession(string deviceId)
        {
            return SessionManager.Sessions.FirstOrDefault(i => i.DeviceId == deviceId);
        }

        private void BrowseHome(string room, User user, string deviceId = null, SessionInfo session = null)
        {
            try
            {
                deviceId = deviceId ?? GetDeviceIdFromRoomName(room);
                session = session ?? GetSession(deviceId);

                //DO NOT AWAIT THIS! ALEXA HATES YOU FOR IT
                SessionManager.SendGeneralCommand(null, session.Id, new GeneralCommand()
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

        public void BrowseItemAsync(string room, User user, BaseItem request)
        {
            var deviceId = GetDeviceIdFromRoomName(room);

            if (string.IsNullOrEmpty(deviceId))
            {
                throw new Exception(SemanticSpeechStrings.GetPhrase(SpeechResponseType.DEVICE_UNAVAILABLE, null));
            }

            var session = GetSession(deviceId);

            var type = request.GetType().Name;
           
            if (!type.Equals("Season") || !type.Equals("Series")) BrowseHome(room, user, deviceId, session);

            Task.Delay(1500).Wait();

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
                throw new Exception($"I was unable to browse to {request.Name}.");
            }
        }

        public async void PlayMediaItemAsync(IAlexaSession alexaSession, BaseItem item, User user)
        {
            var deviceId = GetDeviceIdFromRoomName(alexaSession.room.Name);
            
            if (string.IsNullOrEmpty(deviceId))
            {
                throw new Exception(SemanticSpeechStrings.GetPhrase(SpeechResponseType.DEVICE_UNAVAILABLE, alexaSession));
            }

            var session  = GetSession(deviceId);

            if (session is null) throw new Exception(SemanticSpeechStrings.GetPhrase(SpeechResponseType.DEVICE_UNAVAILABLE, alexaSession));

            
            // ReSharper disable once TooManyChainedReferences
            long startTicks = item.SupportsPositionTicksResume ? item.PlaybackPositionTicks : 0;


            await SessionManager.SendPlayCommand(null, session.Id, new PlayRequest
            {
                StartPositionTicks = startTicks,
                PlayCommand        = PlayCommand.PlayNow,
                ItemIds            = new[] { item.InternalId },
                ControllingUserId  = user.Id.ToString()

            }, CancellationToken.None);

        }

        
        public void Dispose()
        {
            
        }

        public void Run()
        {
            
        }

        
    }
}
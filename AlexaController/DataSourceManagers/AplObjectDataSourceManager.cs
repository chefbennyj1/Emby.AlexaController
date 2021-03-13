using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Transformers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

namespace AlexaController.DataSourceManagers
{
    public class AplObjectDataSourceManager
    {
        public static AplObjectDataSourceManager Instance { get; private set; }

        public AplObjectDataSourceManager()
        {
            Instance = this;
        }
        
        public async Task<IDataSource> GetSequenceItemsDataSourceAsync(List<BaseItem> collectionItems, BaseItem collection = null)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var mediaItems = new List<MediaItem>();

            //All the items in the list are of the same type, grab the first one and get the type
            var type = collectionItems[0].GetType().Name;

            collectionItems.ForEach(i => mediaItems.Add(new MediaItem()
            {
                type                = type,
                primaryImageSource  = ServerQuery.Instance.GetPrimaryImageSource(i),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(i),
                id                  = i.InternalId,
                name                = i.Name,
                index               = type == "Episode" ? $"Episode {i.IndexNumber}" : string.Empty,
                premiereDate        = i.PremiereDate?.ToString("D")

            }));

            MediaItem mediaItem = null;
            if (collection != null)
            {
                mediaItem = new MediaItem()
                {
                    name                = textInfo.ToTitleCase(collection.Name.ToLower()),
                    logoImageSource     = ServerQuery.Instance.GetLogoImageSource(collection),
                    backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(collection),
                    id                  = collection.InternalId,
                    genres              = ServerQuery.Instance.GetGenres(collection)
                };
            }

            return await Task.FromResult(new DataSource<MediaItem>()
            {
                properties = new Properties<MediaItem>()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                    items = mediaItems,
                    item = mediaItem
                },
                transformers = new List<ITransformer>()
                {
                    new AplaSpeechTransformer()
                    {
                        template = "buttonPressEffectApla",
                        outputName = "buttonPressEffect"
                    }
                }

            });
        }

        public async Task<IDataSource> GetBaseItemDetailsDataSourceAsync(BaseItem item, IAlexaSession session)
        {
            var mediaItem = new MediaItem()
            {
                type                = item.GetType().Name,
                isPlayed            = item.IsPlayed(session.User),
                primaryImageSource  = ServerQuery.Instance.GetPrimaryImageSource(item),
                id                  = item.InternalId,
                name                = item.Name,
                premiereDate        = item.ProductionYear.ToString(),
                officialRating      = item.OfficialRating,
                tagLine             = item.Tagline,
                runtimeMinutes      = ServerQuery.Instance.GetRunTime(item),
                endTime             = ServerQuery.Instance.GetEndTime(item),
                genres              = ServerQuery.Instance.GetGenres(item),
                logoImageSource     = ServerQuery.Instance.GetLogoImageSource(item),
                overview            = item.Overview,
                videoBackdropSource = ServerQuery.Instance.GetVideoBackdropImageSource(item),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(item),
                videoOverlaySource  = "/EmptyPng?quality=90",
                themeAudioSource    =  ServerQuery.Instance.GetThemeSongSource(item)
            };

            var similarItems = ServerQuery.Instance.GetSimilarItems(item);

            var recommendedItems = new List<MediaItem>();

            similarItems.ForEach(r => recommendedItems.Add(new MediaItem()
            {
                id = r.InternalId,
                thumbImageSource = ServerQuery.Instance.GetThumbImageSource(r)
            }));

            return await Task.FromResult(new DataSource<MediaItem>()
            {
                properties = new Properties<MediaItem>()
                {
                    url          = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                    similarItems = recommendedItems,
                    item         = mediaItem
                },
                transformers = new List<ITransformer>()
                {
                    new TextToSpeechTransformer()
                    {
                        inputPath = "item.overview",
                        outputName = "readOverview"
                    }
                }
            });

        }

        public async Task<IDataSource> GetHelpDataSourceAsync()
        {
            var helpContent = new List<Value>()
            {
                 new Value() {value = "Accessing Emby accounts based on your voice."},
                 new Value() {value = "I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now."},
                 new Value() {value = "You can enable Parental Controls, so media will be filtered based on who is speaking. This way, media items will be filtered using voice verification."},
                 new Value() {value = "To enable this feature, open the Emby plugin configuration page, and toggle the button for \"Enable parental control using voice recognition\". If this feature is turned off, I will not filter media items, and will show media based on the Emby Administrators account, at all times."},
                 new Value() {value = "Select the \"New Authorization Account Link\" button, and follow the instructions to add personalization."},
                 new Value() {value = "Accessing media Items in rooms"},
                 new Value() {value = "The Emby plugin will allow you to create \"Rooms\", based on the devices in your home."},
                 new Value() {value = "In the plugin configuration, map each Emby ready device to a specific room. You will create the room name that I will understand."},
                 new Value() {value = "For example: map your Amazon Fire Stick 4K to a room named: \"Family Room\"."},
                 new Value() {value = "Now you can access titles and request them to display per room."},
                 new Value() {value = "You can use the phrase:"},
                 new Value() {value = "Ask home theater to play the movie Iron Man in the family room."},
                 new Value() {value = "To display the movies library on the \"Family Room\" device, you can use the phrase:"},
                 new Value() {value = "Ask home theater to show \"Movies\" in the Family Room."},
                 new Value() {value = "The same can be said for \"Collections\", and \"TV Series\" libraries"},
                 new Value() {value = "The Emby client must already be running on the device in order to access the room commands."},
                 new Value() {value = "Accessing Collection Items"},
                 new Value() {value = "I have the ability to show collection data on echo show, echo spot, or other devices with screens."},
                 new Value() {value = "To access this ability, you can use the following phrases:"},
                 new Value() {value = "Ask home theater to show all the Iron Man movies..."},
                 new Value() {value = "Ask home theater to show the Spiderman collection..."},
                 new Value() {value = "Fire TV devices, with Alexa enabled, are exempt from displaying these items because the Emby client will take care of this for you."},
                 new Value() {value = "Accessing individual media Items"},
                 new Value() {value = "I am able to show individual media items as well."},
                 new Value() {value = "You can use the phrases:"},
                 new Value() {value = "Ask home theater to show the movie Spiderman Far From Home..."},
                 new Value() {value = "Ask home theater to show the movie Spiderman Far From Home, in the Family Room."},
                 new Value() {value = "You can also access TV Series the same way."},
                 new Value() {value = "I will understand the phrases: "},
                 new Value() {value = "Ask home theater to show the series Westworld..."},
                 new Value() {value = "Ask home theater to show the next up episode for Westworld..."},
                 new Value() {value = "Accessing new media Items"},
                 new Value() {value = "To access new titles in your library, you can use the phrases:"},
                 new Value() {value = "Ask home theater about new movies..."},
                 new Value() {value = "Ask home theater about new TV Series..."},
                 new Value() {value = "You can also request a duration for new items... For example:"},
                 new Value() {value = "Ask home theater for new movies added in the last three days..."},
                 new Value() {value = "Ask home theater for new tv shows added in the last month"},
                 new Value() {value = "Remember that as long as the echo show, or spot is displaying an image, you are in an open session. This means you don't have to use the \"Ask home theater\" phrases to access media"},
                 new Value() {value = "This concludes the help section. Good luck!"},
            };

            return await Task.FromResult(new DataSource<List<Value>>()
            {
                properties = new Properties<List<Value>>
                {
                    documentType = RenderDocumentType.HELP,
                    values = helpContent
                },
                transformers = new List<ITransformer>()
                {
                    new TextToSpeechTransformer()
                    {
                        inputPath = "values[*].value",
                        outputName = "helpPhrase"
                    }
                }
            });
        }
        
        public async Task<IDataSource> GetGenericViewDataSource(string text, string videoUrl)
        {
            return await Task.FromResult(new DataSource<string>()
            {
                properties = new Properties<string>()
                {
                    documentType = RenderDocumentType.GENERIC_VIEW,
                    text         = text,
                    url          = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    videoUrl     = videoUrl
                }
            });
        }
        
        public async Task<IDataSource> GetRoomSelection(BaseItem item, IAlexaSession session) 
        {
            return await Task.FromResult(new DataSource<MediaItem>()
            {
                properties = new Properties<MediaItem>()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ROOM_SELECTION_TEMPLATE,
                    item = new MediaItem()
                    {
                        type                = item.GetType().Name,
                        isPlayed            = item.IsPlayed(session.User),
                        primaryImageSource  = ServerQuery.Instance.GetPrimaryImageSource(item),
                        id                  = item.InternalId,
                        name                = item.Name,
                        premiereDate        = item.ProductionYear.ToString(),
                        officialRating      = item.OfficialRating,
                        tagLine             = item.Tagline,
                        runtimeMinutes      = ServerQuery.Instance.GetRunTime(item),
                        endTime             = ServerQuery.Instance.GetEndTime(item),
                        genres              = ServerQuery.Instance.GetGenres(item),
                        logoImageSource     = ServerQuery.Instance.GetLogoImageSource(item),
                        overview            = item.Overview,
                        videoBackdropSource = ServerQuery.Instance.GetVideoBackdropImageSource(item),
                        backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(item)
                    }
                }
            });
        }

    }
}

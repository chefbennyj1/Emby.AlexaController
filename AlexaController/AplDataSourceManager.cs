using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.DataSourceProperties;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

namespace AlexaController
{
    

    public class AplDataSourceManager
    {

        public static AplDataSourceManager Instance { get; private set; }

        public AplDataSourceManager()
        {
            Instance = this;
        }

        //Data Sources
        public async Task<IDataSource> GetSequenceItemsDataSourceAsync(List<BaseItem> collectionItems, BaseItem collection = null)
        {
            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var dataSourceItems = new List<MediaItem>();

            //All the items in the list are of the same type, grab the first one and get the type
            var type = collectionItems[0].GetType().Name;

            collectionItems.ForEach(i => dataSourceItems.Add(new MediaItem()
            {
                type = type,
                primaryImageSource = ServerQuery.Instance.GetPrimaryImageSource(i),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(i),
                id = i.InternalId,
                name = i.Name,
                index = type == "Episode" ? $"Episode {i.IndexNumber}" : string.Empty,
                premiereDate = i.PremiereDate?.ToString("D")

            }));

            MediaItem dataSourceItem = null;
            if (collection != null)
            {
                dataSourceItem = new MediaItem()
                {
                    name = textInfo.ToTitleCase(collection.Name.ToLower()),
                    logoImageSource = ServerQuery.Instance.GetLogoImageSource(collection),
                    backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(collection),
                    id = collection.InternalId,
                    genres = ServerQuery.Instance.GetGenres(collection)
                };
            }

            var dataSource = new DataSource()
            {
                properties = new Properties<MediaItem>()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                    items = dataSourceItems,
                    item = dataSourceItem
                }
            };

            return await Task.FromResult(dataSource);
        }

        public async Task<IDataSource> GetBaseItemDetailsDataSourceAsync(BaseItem item, IAlexaSession session)
        {
            var dataSourceItem = new MediaItem()
            {
                type = item.GetType().Name,
                isPlayed = item.IsPlayed(session.User),
                primaryImageSource = ServerQuery.Instance.GetPrimaryImageSource(item),
                id = item.InternalId,
                name = item.Name,
                premiereDate = item.ProductionYear.ToString(),
                officialRating = item.OfficialRating,
                tagLine = item.Tagline,
                runtimeMinutes = ServerQuery.Instance.GetRunTime(item),
                endTime = ServerQuery.Instance.GetEndTime(item),
                genres = ServerQuery.Instance.GetGenres(item),
                logoImageSource = ServerQuery.Instance.GetLogoImageSource(item),
                overview = item.Overview,
                videoBackdropSource = ServerQuery.Instance.GetVideoBackdropImageSource(item),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(item),
                videoOverlaySource = "/EmptyPng?quality=90"
            };

            var similarItems = ServerQuery.Instance.GetSimilarItems(item);

            var recommendedItems = new List<MediaItem>();

            similarItems.ForEach(r => recommendedItems.Add(new MediaItem()
            {
                id = r.InternalId,
                thumbImageSource = ServerQuery.Instance.GetThumbImageSource(r)
            }));

            var dataSource = new DataSource()
            {
                properties = new Properties<MediaItem>()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                    similarItems = recommendedItems,
                    item = dataSourceItem
                }
            };

            return await Task.FromResult(dataSource);
        }

        public async Task<IDataSource> GetHelpDataSourceAsync<T>()
        {
            var helpContent = new List<Value<string>>()
            {
                new Value<string>() {value = "<b>Accessing Emby accounts based on your voice.</b>"},
                new Value<string>() {value = "I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now."},
                new Value<string>() {value = "You can enable Parental Controls, so media will be filtered based on who is speaking. This way, media items will be filtered using voice verification."},
                new Value<string>() {value = "To enable this feature, open the Emby plugin configuration page, and toggle the button for \"Enable parental control using voice recognition\". If this feature is turned off, I will not filter media items, and will show media based on the Emby Administrators account, at all times."},
                new Value<string>() {value = "Select the \"New Authorization Account Link\" button, and follow the instructions to add personalization."},
                new Value<string>() {value = "<b>Accessing media Items in rooms</b>"},
                new Value<string>() {value = "The Emby plugin will allow you to create \"Rooms\", based on the devices in your home."},
                new Value<string>() {value = "In the plugin configuration, map each Emby ready device to a specific room. You will create the room name that I will understand."},
                new Value<string>() {value = "For example: map your Amazon Fire Stick 4K to a room named: \"Family Room\"."},
                new Value<string>() {value = "Now you can access titles and request them to display per room."},
                new Value<string>() {value = "You can use the phrase:"},
                new Value<string>() {value = "Ask home theater to play the movie Iron Man in the family room."},
                new Value<string>() {value = "To display the movies library on the \"Family Room\" device, you can use the phrase:"},
                new Value<string>() {value = "Ask home theater to show \"Movies\" in the Family Room."},
                new Value<string>() {value = "The same can be said for \"Collections\", and \"TV Series\" libraries"},
                new Value<string>() {value = "The Emby client must already be running on the device in order to access the room commands."},
                new Value<string>() {value = "<b>Accessing Collection Items</b>"},
                new Value<string>() {value = "I have the ability to show collection data on echo show, echo spot, or other devices with screens."},
                new Value<string>() {value = "To access this ability, you can use the following phrases:"},
                new Value<string>() {value = "Ask home theater to show all the Iron Man movies..."},
                new Value<string>() {value = "Ask home theater to show the Spiderman collection..."},
                new Value<string>() {value = "Fire TV devices, with Alexa enabled, are exempt from displaying these items because the Emby client will take care of this for you."},
                new Value<string>() {value = "<b>Accessing individual media Items</b>"},
                new Value<string>() {value = "I am able to show individual media items as well."},
                new Value<string>() {value = "You can use the phrases:"},
                new Value<string>() {value = "Ask home theater to show the movie Spiderman Far From Home..."},
                new Value<string>() {value = "Ask home theater to show the movie Spiderman Far From Home, in the Family Room."},
                new Value<string>() {value = "You can also access TV Series the same way."},
                new Value<string>() {value = "I will understand the phrases: "},
                new Value<string>() {value = "Ask home theater to show the series Westworld..."},
                new Value<string>() {value = "Ask home theater to show the next up episode for Westworld..."},
                new Value<string>() {value = "<b>Accessing new media Items</b>"},
                new Value<string>() {value = "To access new titles in your library, you can use the phrases:"},
                new Value<string>() {value = "Ask home theater about new movies..."},
                new Value<string>() {value = "Ask home theater about new TV Series..."},
                new Value<string>() {value = "You can also request a duration for new items... For example:"},
                new Value<string>() {value = "Ask home theater for new movies added in the last three days..."},
                new Value<string>() {value = "Ask home theater for new tv shows added in the last month"},
                new Value<string>() {value = "Remember that as long as the echo show, or spot is displaying an image, you are in an open session. This means you don't have to use the \"Ask home theater\" phrases to access media"},
                new Value<string>() {value = "This concludes the help section. Good luck!"},
            };

            return await Task.FromResult(new DataSource()
            {
                properties = new Properties<List<Value<T>>>
                {
                    documentType = RenderDocumentType.HELP,
                    values = helpContent
                }
            });
        }

        public async Task<IDataSource> GetNotUnderstood()
        {
            return await Task.FromResult(new DataSource()
            {
                properties = new Properties<BaseProperties>()
                {
                    documentType = RenderDocumentType.NOT_UNDERSTOOD,
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync()
                }
            });
        }

        public async Task<IDataSource> GetRoomSelection(BaseItem item, IAlexaSession session)
        {
            return await Task.FromResult(new DataSource()
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

        public async Task<IDataSource> GetGenericHeadline(string content)
        {
            return await Task.FromResult(new DataSource()
            {
                properties = new Properties<BaseProperties>()
                {
                    documentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                    HeadlinePrimaryText = content
                }
            });
        }

        public async Task<IDataSource> GetBrowseLibrary(string content)
        {
            return await Task.FromResult(new DataSource()
            {
                properties = new Properties<BaseProperties>()
                {
                    documentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE,
                    HeadlinePrimaryText = content,
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync()
                }
            });
        }

        public async Task<IDataSource> GetFollowUpQuestion(string content)
        {
            return await Task.FromResult(new DataSource()
            {
                properties = new Properties<BaseProperties>()
                {
                    documentType = RenderDocumentType.FOLLOW_UP_QUESTION,
                    HeadlinePrimaryText = content,
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync()
                }
            });
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSourceModel;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa.Presentation.DirectiveBuilders
{
    public class DataSourceManager
    {
        public static DataSourceManager Instance { get; private set; }
        public DataSourceManager()
        {
            Instance = this;
        }

        //Data Sources
        public async Task<Dictionary<string, IDataSource>> GetSequenceItemsDataSourceAsync(string dataSourceKey, List<BaseItem> sequenceItems)
        {
            var dataSource = new Dictionary<string, IDataSource>();
            var dataSourceItems = new List<MediaItem>();

            //All the items in the list are of the same type, grab the first one and get the type
            var type = sequenceItems[0].GetType().Name;

            sequenceItems.ForEach(i => dataSourceItems.Add(new MediaItem()
            {
                type = type,
                primaryImageSource = ServerQuery.Instance.GetPrimaryImageSource(i),
                backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(i),
                id = i.InternalId,
                name = i.Name,
                index = type == "Episode" ? $"Episode {i.IndexNumber}" : string.Empty,
                premiereDate = i.PremiereDate?.ToString("D")

            }));

            dataSource.Add(dataSourceKey, new MediaItemDataSource()
            {
                properties = new MediaItemDataSourceProperties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    items = dataSourceItems
                }
            });
            ServerController.Instance.Log.Info("Render Document Sequence has Data Source");
            return await Task.FromResult(dataSource);
        }

        public async Task<Dictionary<string, IDataSource>> GetBaseItemDetailsDataSourceAsync(string dataSourceKey, BaseItem item)
        {
            var dataSource = new Dictionary<string, IDataSource>();

            var dataSourceItem = new MediaItem()
            {
                type = item.GetType().Name,
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

            var recommendedItems = new List<SimilarItem>();

            similarItems.ForEach(r => recommendedItems.Add(new SimilarItem()
            {

                id = r.InternalId,
                thumbImageSource = ServerQuery.Instance.GetThumbImageSource(r)

            }));

            dataSource.Add(dataSourceKey, new MediaItemDataSource()
            {
                properties = new MediaItemDataSourceProperties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    similarItems = recommendedItems,
                    item = dataSourceItem
                }
            });

            ServerController.Instance.Log.Info("Render Document has Data Source");
            return await Task.FromResult(dataSource);
        }

        public async Task<Dictionary<string, IDataSource>> GetHelpDataSourceAsync(string dataSourceKey)
        {
            var helpProperties = new HelpDataSourceProperties
            {
                helpContent = new List<HelpValues>()
                {
                    new HelpValues(){ value = "<b>Accessing Emby accounts based on your voice.</b>"},
                    new HelpValues(){ value ="I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now."},
                    new HelpValues(){ value ="You can enable Parental Controls, so media will be filtered based on who is speaking. This way, media items can be parental controlled using voice verification."},
                    new HelpValues(){ value ="To enable this feature, open the Emby plugin configuration page, and toggle the button for \"Enable parental control using voice recognition\". If this feature is turned off, I will not filter media items, and will show media based on the Emby Administrators account, at all times."},
                    new HelpValues(){ value ="Select the \"New Authorization Account Link\" button, and follow the instructions to add personalization."},
                    new HelpValues(){ value ="<b>Accessing media Items in rooms</b>"},
                    new HelpValues(){ value ="The Emby plugin will allow you to create \"Rooms\", based on the devices in your home."},
                    new HelpValues(){ value ="In the plugin configuration, map each Emby ready device to a specific room. You will create the room name that I will understand."},
                    new HelpValues(){ value ="For example: map your Amazon Fire Stick 4K to a room named: \"Family Room\"."},
                    new HelpValues(){ value ="Now you can access titles and request them to display per room."},
                    new HelpValues(){ value ="You can use the phrase:"},
                    new HelpValues(){ value ="Ask home theater to play the movie Iron Man in the family room."},
                    new HelpValues(){ value ="To display the movies library on the \"Family Room\" device, you can use the phrase:"},
                    new HelpValues(){ value ="Ask home theater to show \"Movies\" in the Family Room."},
                    new HelpValues(){ value ="The same can be said for \"Collections\", and \"TV Series\" libraries"},
                    new HelpValues(){ value = "The Emby client must already be running on the device in order to access the room commands."},
                    new HelpValues(){ value = "<b>Accessing Collection Items</b>"},
                    new HelpValues(){ value ="I have the ability to show collection data on echo show, echo spot, or other devices with screens."},
                    new HelpValues(){ value ="To access this ability, you can use the following phrases:"},
                    new HelpValues(){ value ="Ask home theater to show all the Iron Man movies..."},
                    new HelpValues(){ value ="Ask home theater to show the Spiderman collection..."},
                    new HelpValues(){ value ="Fire TV devices, with Alexa enabled, are exempt from displaying these items because the Emby client will take care of this for you."},
                    new HelpValues(){ value ="<b>Accessing individual media Items</b>"},
                    new HelpValues(){ value ="I am able to show individual media items as well."},
                    new HelpValues(){ value ="You can use the phrases:"},
                    new HelpValues(){ value ="Ask home theater to show the movie Spiderman Far From Home..."},
                    new HelpValues(){ value ="Ask home theater to show the movie Spiderman Far From Home, in the Family Room."},
                    new HelpValues(){ value ="You can also access TV Series the same way."},
                    new HelpValues(){ value ="I will understand the phrases: "},
                    new HelpValues(){ value ="Ask home theater to show the series Westworld..."},
                    new HelpValues(){ value ="Ask home theater to show the next up episode for Westworld..."},
                    new HelpValues(){ value ="<b>Accessing new media Items</b>"},
                    new HelpValues(){ value ="To access new titles in your library, you can use the phrases:"},
                    new HelpValues(){ value ="Ask home theater about new movies..."},
                    new HelpValues(){ value ="Ask home theater about new TV Series..."},
                    new HelpValues(){ value ="You can also request a duration for new items... For example:"},
                    new HelpValues(){ value ="Ask home theater for new movies added in the last three days..."},
                    new HelpValues(){ value ="Ask home theater for new tv shows added in the last month"},
                    new HelpValues(){ value = "Remember that as long as the echo show, or spot is displaying an image, you are in an open session. This means you don't have to use the \"Ask home theater\" phrases to access media"},
                    new HelpValues(){ value ="This concludes the help section. Good luck!"},
                }
            };

            return await Task.FromResult(new Dictionary<string, IDataSource>()
            {
                { dataSourceKey , new HelpDataSource() { properties =  helpProperties } }
            });


        }
    }
}

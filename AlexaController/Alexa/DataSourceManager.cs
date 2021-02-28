using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa
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

            dataSource.Add(dataSourceKey, new DataSource()
            {
                properties = new MediaItemProperties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    items = dataSourceItems
                }
            });
            
            return await Task.FromResult(dataSource);
        }

        public async Task<Dictionary<string, IDataSource>> GetBaseItemDetailsDataSourceAsync(string dataSourceKey, BaseItem item)
        {
            var dataSource = new Dictionary<string, IDataSource>();

            var dataSourceItem = new MediaItem()
            {
                type                = item.GetType().Name,
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
                videoOverlaySource  = "/EmptyPng?quality=90"
            };

            var similarItems = ServerQuery.Instance.GetSimilarItems(item);

            var recommendedItems = new List<SimilarItem>();

            similarItems.ForEach(r => recommendedItems.Add(new SimilarItem()
            {
                id = r.InternalId,
                thumbImageSource = ServerQuery.Instance.GetThumbImageSource(r)
            }));

            dataSource.Add(dataSourceKey, new DataSource()
            {
                properties = new MediaItemProperties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    similarItems = recommendedItems,
                    item = dataSourceItem
                }
            });
            
            return await Task.FromResult(dataSource);
        }

        public async Task<Dictionary<string, IDataSource>> GetHelpDataSourceAsync(string dataSourceKey)
        {
            return await Task.FromResult(new Dictionary<string, IDataSource>()
            {
                {
                    dataSourceKey, new DataSource()
                    {
                        properties = new HelpProperties()
                    }
                }
            });


        }
    }
}

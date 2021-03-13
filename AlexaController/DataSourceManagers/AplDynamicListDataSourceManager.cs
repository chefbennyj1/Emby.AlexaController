using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.DataSourceManagers.DataSourceProperties;
using MediaBrowser.Controller.Entities;

namespace AlexaController.DataSourceManagers
{
    public class AplDynamicListDataSourceManager
    {
        public static AplDynamicListDataSourceManager Instance { get; set; }
        public AplDynamicListDataSourceManager()
        {
            Instance = this;
        }

        //public async Task<IDataSource> GetDynamicListSequenceItemDataSourceAsync(List<BaseItem> collectionsItems, BaseItem collection)
        //{
        //    var mediaItems = new List<MediaItem>();
        //    var type = collectionsItems[0].GetType().Name;
        //    collectionsItems.ForEach(i => mediaItems.Add(new MediaItem()
        //    {
        //        type                = type,
        //        primaryImageSource  = ServerQuery.Instance.GetPrimaryImageSource(i),
        //        backdropImageSource = ServerQuery.Instance.GetBackdropImageSource(i),
        //        id                  = i.InternalId,
        //        name                = i.Name,
        //        index               = type == "Episode" ? $"Episode {i.IndexNumber}" : string.Empty,
        //        premiereDate        = i.PremiereDate?.ToString("D")

        //    }));

            //return await Task.FromResult(new DynamicIndexListDataSource<MediaItem>()
            //{
            //    items = mediaItems,
            //    startIndex = 0,
            //    minimumInclusiveIndex = 0,
            //    listId = type
//            //});
//        }
    }
}

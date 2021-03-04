using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.DataSourceProperties;
using AlexaController.DataSourceProperties.AplDataSourceProperties;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

namespace AlexaController
{
    //TODO try to move this into IProperties interface
    public enum RenderDocumentType
    {
        NONE,
        ITEM_DETAILS_TEMPLATE,
        ITEM_LIST_SEQUENCE_TEMPLATE,
        BROWSE_LIBRARY_TEMPLATE,
        FOLLOW_UP_QUESTION,
        APLA,
        NOT_UNDERSTOOD,
        HELP,
        GENERIC_HEADLINE_TEMPLATE,
        ROOM_SELECTION_TEMPLATE,
        VERTICAL_TEXT_LIST_TEMPLATE
    }

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
                properties = new MediaItemProperties()
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

            var recommendedItems = new List<SimilarItem>();

            similarItems.ForEach(r => recommendedItems.Add(new SimilarItem()
            {
                id = r.InternalId,
                thumbImageSource = ServerQuery.Instance.GetThumbImageSource(r)
            }));

            var dataSource = new DataSource()
            {
                properties = new MediaItemProperties()
                {
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync(),
                    documentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                    similarItems = recommendedItems,
                    item = dataSourceItem
                }
            };

            return await Task.FromResult(dataSource);
        }

        public async Task<IDataSource> GetHelpDataSourceAsync()
        {
            //TODO We may need properties with URL here!!
            return await Task.FromResult(new DataSource()
            {
                properties = new HelpProperties()
            });
        }

        public async Task<IDataSource> GetNotUnderstood()
        {
            return await Task.FromResult(new DataSource()
            {
                properties = new GenericHeadlineProperties()
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
                properties = new MediaItemProperties()
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
                properties = new GenericHeadlineProperties()
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
                properties = new GenericHeadlineProperties()
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
                properties = new GenericHeadlineProperties()
                {
                    documentType = RenderDocumentType.FOLLOW_UP_QUESTION,
                    HeadlinePrimaryText = content,
                    url = await ServerQuery.Instance.GetLocalApiUrlAsync()
                }
            });
        }
    }
}

using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;


namespace AlexaController.Alexa.IntentRequest.Browse
{

    public class CollectionIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public CollectionIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestContextData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;

            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);

            var collection = ServerQuery.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems = collection.Values.FirstOrDefault();
            var collectionBaseItem = collection.Keys.FirstOrDefault();

            IDataSource aplDataSource = null;
            IDataSource aplaDataSource = null;

            //Parental Control check for baseItem
            if (!(collectionBaseItem is null))
            {
                if (!collectionBaseItem.IsParentalAllowed(Session.User))
                {
                    if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                    {
                        await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                            $"{Session.User} attempted to view a restricted item.", $"{Session.User} attempted to view {collectionBaseItem.Name}.").ConfigureAwait(false);
                    }

                    aplDataSource = await APL_DataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {collectionBaseItem.OfficialRating}", "/particles");
                    aplaDataSource = await APLA_DataSourceManager.Instance.ParentalControlNotAllowed(collectionBaseItem, Session);
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await APL_RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, Session),

                            await APLA_RenderDocumentManager.Instance.GetAudioDirectiveAsync(aplaDataSource)

                        }
                    }, Session);
                }
            }

            if (Session.hasRoom)
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, collectionBaseItem);
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            //aplDataSource = await AplDynamicListDataSourceManager.Instance.GetDynamicListSequenceItemDataSourceAsync(collectionItems, collectionBaseItem);
            aplDataSource = await APL_DataSourceManager.Instance.GetSequenceItemsDataSourceAsync(collectionItems, collectionBaseItem);

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await APL_RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{collectionBaseItem?.Name}",
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective
                },

            }, Session);
        }
    }
}



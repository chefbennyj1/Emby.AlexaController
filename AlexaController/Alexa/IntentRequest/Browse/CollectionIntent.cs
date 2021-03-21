using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplDataSourceManagement.PropertyModels;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
            await AlexaResponseClient.Instance.PostProgressiveResponse("OK.",
                AlexaRequest.context.System.apiAccessToken, AlexaRequest.request.requestId);

            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestContextData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;

            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);

            var collection = ServerQuery.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems = collection.Values.FirstOrDefault();
            var collectionBaseItem = collection.Keys.FirstOrDefault();

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

                    var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync($"Stop! Rated {collectionBaseItem.OfficialRating}", "/particles");
                    var aplaDataSource = await DataSourceAudioSpeechPropertiesManager.Instance.ParentalControlNotAllowed(collectionBaseItem, Session);
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,

                        directives = new List<IDirective>()
                        {
                            await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(genericLayoutProperties, Session),
                            await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)

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

            var sequenceLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetSequenceViewPropertiesAsync(collectionItems, collectionBaseItem);

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(sequenceLayoutProperties, Session);

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



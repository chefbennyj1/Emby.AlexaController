using AlexaController.Alexa;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Browse
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
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;

            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);

            var collection = ServerDataQuery.Instance.GetCollectionItems(Session.User, collectionRequest);
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

                    var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync($"Stop! Rated {collectionBaseItem.OfficialRating}", "/particles");
                    var aplaDataSource = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
                    {
                        SpeechResponseType = SpeechResponseType.ParentalControlNotAllowed,
                        item = collectionBaseItem,
                        session = Session

                    });
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,

                        directives = new List<IDirective>()
                        {
                            await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, Session),
                            await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource)

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

            var sequenceLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemCollectionSequenceViewPropertiesAsync(collectionItems, collectionBaseItem);

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(sequenceLayoutProperties, Session);

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



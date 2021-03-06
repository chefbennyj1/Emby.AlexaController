using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
   
    public class CollectionIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }

        public CollectionIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }
        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch (Exception exception)
            {
                ServerController.Instance.Log.Error(exception.Message);
            }

           
            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = AlexaRequest.context;
            

            ServerController.Instance.Log.Info(nameof(CollectionIntent) + " request: " + collectionRequest);
            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);
            ServerController.Instance.Log.Info(nameof(CollectionIntent) + " normalized request: " + collectionRequest);
            
            var collection          = ServerQuery.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems     = collection.Values.FirstOrDefault();
            var collectionBaseItem  = collection.Keys.FirstOrDefault();

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

                    aplDataSource = await AplDataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {collectionBaseItem.OfficialRating}", "/particles");
                    aplaDataSource = await AplaDataSourceManager.Instance.ParentalControlNotAllowed(collectionBaseItem, Session);
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(aplDataSource, Session),
                                
                            await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                           
                        }
                    }, Session);
                }
            }

            if (!(Session.room is null))
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

            
            aplDataSource = await AplDataSourceManager.Instance.GetSequenceItemsDataSourceAsync(collectionItems, collectionBaseItem);

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(aplDataSource, Session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{collectionBaseItem.Name}",
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
   


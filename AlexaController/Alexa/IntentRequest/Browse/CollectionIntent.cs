using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
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

            IDataSource dataSource = null;
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

                    dataSource = await DataSourceManager.Instance.GetGenericHeadline($"Stop! Rated {collectionBaseItem.OfficialRating}");
                    
                    return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session),
                                
                            await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(
                                new AudioDirectiveQuery()
                                {
                                    speechContent = SpeechContent.PARENTAL_CONTROL_NOT_ALLOWED,
                                    session = Session, 
                                    items   =  new List<BaseItem>(){ collectionBaseItem },
                                    audio = new Audio()
                                    {
                                        source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                    }
                                })
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

            
            dataSource = await DataSourceManager.Instance.GetSequenceItemsDataSourceAsync(collectionItems, collectionBaseItem);

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, dataSource);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session);

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
   


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Model.RequestData;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class CollectionIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }

        public CollectionIntent(IAlexaRequest alexaRequest, IAlexaSession session)
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
            catch { }

           
            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            
#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse("One Moment Please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            ServerController.Instance.Log.Info(nameof(CollectionIntent) + " request: " + collectionRequest);
            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);
            ServerController.Instance.Log.Info(nameof(CollectionIntent) + " normalized request: " + collectionRequest);
            
            var collection          = ServerQuery.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems     = collection.Values.FirstOrDefault();
            var collectionBaseItem  = collection.Keys.FirstOrDefault();
            
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

                    return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(
                                new InternalRenderDocumentQuery()
                                {
                                    renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                                    HeadlinePrimaryText = "Stop!"

                                }, Session),
                            await RenderAudioManager.Instance.GetAudioDirectiveAsync(
                                new InternalRenderAudioQuery()
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
#pragma warning disable 4014
                    Task.Run(() => 
                            ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            ServerController.Instance.Log.Info(nameof(CollectionIntent) + "Preparing collection base item: " + collectionBaseItem?.Name);

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var documentTemplateInfo = new InternalRenderDocumentQuery()
            {
                HeaderTitle            = textInfo.ToTitleCase(collectionBaseItem?.Name.ToLower() ?? throw new Exception("no collection item")),
                renderDocumentType     = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                baseItems              = collectionItems,
                HeaderAttributionImage = collectionBaseItem.HasImage(ImageType.Logo) ? $"/Items/{collectionBaseItem.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null 
            };

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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
   


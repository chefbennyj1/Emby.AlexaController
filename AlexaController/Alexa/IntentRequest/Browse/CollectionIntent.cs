using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;
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

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = Session
            });

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            EmbyServerEntryPoint.Instance.Log.Info(nameof(CollectionIntent) + " request: " + collectionRequest);
            collectionRequest = StringNormalization.ValidateSpeechQueryString(collectionRequest);
            EmbyServerEntryPoint.Instance.Log.Info(nameof(CollectionIntent) + " normalized request: " + collectionRequest);
            
            var collection          = EmbyServerEntryPoint.Instance.GetCollectionItems(Session.User, collectionRequest);

            EmbyServerEntryPoint.Instance.Log.Info(nameof(CollectionIntent) + " collection: " + collection.Keys.FirstOrDefault().Name);
            var collectionItems     = collection.Values.FirstOrDefault();
            var collectionBaseItem  = collection.Keys.FirstOrDefault();
            
            //Parental Control check for baseItem
            if (!(collectionBaseItem is null))
            {
                if (!collectionBaseItem.IsParentalAllowed(Session.User))
                {
                    if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                    {
                        await EmbyServerEntryPoint.Instance.CreateActivityEntry(LogSeverity.Warn,
                            $"{Session.User} attempted to view a restricted item.", $"{Session.User} attempted to view {collectionBaseItem.Name}.").ConfigureAwait(false);
                    }

                    return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                            {
                                type = SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, 
                                session = Session, 
                                items =  new List<BaseItem>(){ collectionBaseItem }
                            }),
                            sound  = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                        }
                    }, Session);
                }
            }

            if (!(Session.room is null))
            {
                try
                {
                    await EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, collectionBaseItem);
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

            EmbyServerEntryPoint.Instance.Log.Info(nameof(CollectionIntent) + "Preparing collection base item: " + collectionBaseItem?.Name);

            var textInfo = CultureInfo.CurrentCulture.TextInfo;
            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                HeaderTitle            = textInfo.ToTitleCase(collectionBaseItem?.Name.ToLower() ?? throw new Exception("no collection item")),
                renderDocumentType     = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                baseItems              = collectionItems,
                HeaderAttributionImage = collectionBaseItem.HasImage(ImageType.Logo) ? $"/Items/{collectionBaseItem.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null 
            };

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
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
   


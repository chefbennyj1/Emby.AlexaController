using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class CollectionIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public CollectionIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
            
        }
        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);


            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;
            
            var progressiveSpeech = $"{SpeechSemantics.SpeechResponse(SpeechType.REPOSE)}";
            ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);
            
            collectionRequest       = StringNormalization.ValidateSpeechQueryString(collectionRequest);
            
            var collection          = EmbyServerEntryPoint.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems     = collection.Items;
            var collectionBaseItem  = EmbyServerEntryPoint.Instance.GetItemById(collection.Id);
            
            //Parental Control check for baseItem
            if (!(collectionBaseItem is null))
            {
                if (!collectionBaseItem.IsParentalAllowed(Session.User))
                {
                    return ResponseClient.Instance.BuildAlexaResponse(new Response()
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>(){ collectionBaseItem }),
                            sound  = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                        }
                    }).Result;
                }
            }

            if (!(Session.room is null))
                try
                {
                    EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, collectionBaseItem);
                }
                catch (Exception exception)
                {
                    ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    Session.room = null;
                }

            await Task.Delay(1200);

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                HeaderTitle            = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(collectionBaseItem.Name.ToLower()),
                renderDocumentType     = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                baseItems              = collectionItems,
                HeaderAttributionImage = collectionBaseItem.HasImage(ImageType.Logo) ? $"/Items/{collectionBaseItem.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null 
            };

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                person       = Session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase         = $"{collectionBaseItem.Name}",
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentAsync(documentTemplateInfo, Session)
                },

            }, Session.alexaSessionDisplayType);
        }
    }
}
   


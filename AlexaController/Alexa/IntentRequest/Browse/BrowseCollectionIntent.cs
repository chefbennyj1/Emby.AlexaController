using System;
using System.Collections.Generic;
using System.Globalization;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BrowseCollectionIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public BrowseCollectionIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);
            
            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;
            
            var progressiveSpeech = $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            Alexa.ResponseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);
            
            collectionRequest       = StringNormalization.NormalizeText(collectionRequest);
            
            var collection          = EmbyControllerUtility.Instance.GetCollectionItems(Session.User, collectionRequest);
            var collectionItems     = collection.Items;
            var collectionBaseItem  = Alexa.LibraryManager.GetItemById(collection.Id);
            
            //Parental Control check for baseItem
            if (!(collectionBaseItem is null))
            {
                if (!collectionBaseItem.IsParentalAllowed(Session.User))
                {
                    return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>(){ collectionBaseItem }),
                            sound  = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                        }
                    });
                }
            }

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, Session.User, collectionBaseItem);
                }
                catch (Exception exception)
                {
                    Alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {
                HeaderTitle        = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(collectionBaseItem.Name.ToLower()),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                baseItems          = collectionItems,
                collectionRoot     = collectionBaseItem
            };

            //Update Session
            Session.NowViewingBaseItem = collectionBaseItem;
            Session.room = room;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                person       = Session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase         = $"{collectionBaseItem.Name}",
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                },

            }, Session.alexaSessionDisplayType);
        }
    }
}
   


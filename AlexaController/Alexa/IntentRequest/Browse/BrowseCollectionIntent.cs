using System.Collections.Generic;
using System.Globalization;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

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
    public class BrowseCollectionIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var roomManager = new RoomContextManager();
            Room room = null;
            try { room = roomManager.ValidateRoom(alexaRequest, session); } catch { }
            var displayNone = Equals(session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return roomManager.RequestRoom(alexaRequest, session, responseClient);
            
            var request           = alexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var collectionRequest = slots.MovieCollection.value ?? slots.Movie.value;
            var context           = alexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;
            
            var progressiveSpeech = $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            responseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);
            
            collectionRequest       = StringNormalization.NormalizeText(collectionRequest);
            
            var collection          = EmbyControllerUtility.Instance.GetCollectionItems(session.User, collectionRequest);
            var collectionItems     = collection.Items;
            var collectionBaseItem  = libraryManager.GetItemById(collection.Id);
            
            //Parental Control check for baseItem
            if (!(collectionBaseItem is null))
            {
                if (!collectionBaseItem.IsParentalAllowed(session.User))
                {
                    return responseClient.BuildAlexaResponse(new Response()
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, session, new List<BaseItem>(){ collectionBaseItem }),
                            sound  = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                        }
                    });
                }
            }
            
            if (!(room is null)) try { EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, collectionBaseItem); } catch { }
            
            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {
                HeaderTitle        = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(collectionBaseItem.Name.ToLower()),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                baseItems          = collectionItems,
                collectionRoot     = collectionBaseItem
            };

            //Update Session
            session.NowViewingBaseItem = collectionBaseItem;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

            return responseClient.BuildAlexaResponse(new Response()
            {
                person       = session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase         = $"{collectionBaseItem.Name}",
                },
                shouldEndSession = null,
                directives       = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                },

            }, session.alexaSessionDisplayType);
        }
    }
}
   


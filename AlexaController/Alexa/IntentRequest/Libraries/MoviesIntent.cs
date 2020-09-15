using System;
using System.Collections.Generic;
using AlexaController.Alexa.Errors;
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

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class MoviesIntent : IntentResponseModel
    {
        
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var config         = Plugin.Instance.Configuration;
            var request        = alexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var libraryName    = "Movies";
            //var room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);//(slots.Room.value ?? session.room) ?? string.Empty;
            var context        = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            Room room = null;
            try
            {
                room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);
            }
            catch
            {
            }

            //we need the room object to proceed because we will only show libraries on emby devices
            if (room is null || (room is null && alexaRequest.context.Viewport is null))
            {
                return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager);
            }
            
            ////do we understand the room object to proceed if it exists
            //if (!string.IsNullOrEmpty(room))
            //{
            //    if (!AlexaSessionManager.Instance.ValidateRoomConfiguration(room, config))
            //        return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager);
            //}

            responseClient.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);

            var result = libraryManager.GetItemById(EmbyControllerUtility.Instance.GetLibraryId(libraryName));
/*
            var resultItems = libraryManager.GetItemsResult(new InternalItemsQuery()
            {
                Parent           = result,
                IncludeItemTypes = new[] { "Movie" },
                Recursive        = true,
                Limit = 20,
                OrderBy          = new[] { new ValueTuple<string, SortOrder>(ItemSortBy.SortName, SortOrder.Ascending) }
            });
*/
            try
            {
                EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, result);
            }
            catch (Exception exception)
            {
                return ErrorHandler.Instance.OnError(exception, alexaRequest, session, responseClient);
            }

            session.NowViewingBaseItem = result;
            
            //reset reprompt data because we have fulfilled the request
            session.PersistedRequestData = null;
            AlexaSessionManager.Instance.UpdateSession(session);

            try
            { 
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_LIBRARY, session, new List<BaseItem>() { result }),
                    },
                    person           = session.person,
                    shouldEndSession = null,
                    directives       = new List<Directive>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            baseItems          = new List<BaseItem>() { result }, //resultItems.Items.ToList(),
                            renderDocumentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE,//RenderDocumentType.VERTICAL_TEXT_LIST_TEMPLATE,
                            HeaderTitle        = "Movies",

                        }, session)
                    },

                }, session.alexaSessionDisplayType);
            }
            catch (Exception)
            {
                return ErrorHandler.Instance.OnError(new Exception("Exception Found"), alexaRequest, session, responseClient);
            }
        }
    }
}

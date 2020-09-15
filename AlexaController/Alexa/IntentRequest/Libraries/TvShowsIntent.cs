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
// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class TvShowsIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var config         = Plugin.Instance.Configuration;
            var request        = alexaRequest.request;
            var libraryName    = "TV Shows";
            //var room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);//(request.intent.slots.Room.value ?? session.room) ?? string.Empty;
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
            if (room is null|| (room is null && alexaRequest.context.Viewport is null))
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

            try
            {
                EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, result);

                session.NowViewingBaseItem = result;
                //reset reprompt data because we have fulfilled the request
                session.PersistedRequestData = null;
                AlexaSessionManager.Instance.UpdateSession(session);

                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_LIBRARY, session, new List<BaseItem>() { result }),
                        semanticSpeechType = SemanticSpeechType.COMPLIANCE,
                    },
                    person           = session.person,
                    shouldEndSession = null,
                    directives       = new List<Directive>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            baseItems          = new List<BaseItem>() { result },
                            renderDocumentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE

                        }, session)
                    },

                }, session.alexaSessionDisplayType);
            }
            catch (Exception exception)
            {
                return ErrorHandler.Instance.OnError(exception, alexaRequest, session, responseClient);
            }
        }
    }
}

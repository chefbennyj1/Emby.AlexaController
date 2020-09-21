using System;
using System.Collections.Generic;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

// ReSharper disable once ConditionIsAlwaysTrueOrFalse

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    public class LibraryIntentResponseManager 
    {
        private string LibraryName { get; }

        public LibraryIntentResponseManager(string libraryName)
        {
            LibraryName = libraryName;
        }

        public string Response(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            
            Room room = null;
            try { room = RoomContextManager.Instance.ValidateRoom(alexaRequest, session); } catch { }
            session.room = room;

            var context = alexaRequest.context;
            // we need the room object to proceed because we will only show libraries on emby devices
            
            if (session.room is null || (session.room is null && context.Viewport is null))
            {
                session.PersistedRequestData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session);
                return RoomContextManager.Instance.RequestRoom(alexaRequest, session);
            }

            var request = alexaRequest.request;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            ResponseClient.Instance.PostProgressiveResponse($"{SpeechSemantics.SpeechResponse(SpeechType.COMPLIANCE)} {SpeechSemantics.SpeechResponse(SpeechType.REPOSE)}", apiAccessToken, requestId);

            var result = EmbyServerEntryPoint.Instance.GetItemById(EmbyServerEntryPoint.Instance.GetLibraryId(LibraryName));

            try
            {
                EmbyServerEntryPoint.Instance.BrowseItemAsync(session, result);
            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(exception, alexaRequest, session);
            }

            session.NowViewingBaseItem = result;
            //reset rePrompt data because we have fulfilled the request
            session.PersistedRequestData = null;
            AlexaSessionManager.Instance.UpdateSession(session);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.BROWSE_LIBRARY, session, new List<BaseItem>() { result }),
                    speechType = SpeechType.COMPLIANCE,
                },
                person = session.person,
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                    {
                        baseItems          = new List<BaseItem>() { result },
                        renderDocumentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE

                    }, session)
                }
            }, session.alexaSessionDisplayType);
        }
    }
}


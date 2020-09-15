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

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BrowseBaseItemDetailsIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            //var config         = Plugin.Instance.Configuration;
            var request        = alexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = request.intent.slots.Movie.value is null ? "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
           //(slots.Room.value ?? session.room) ?? string.Empty;
            var context        = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            Room room = null;
            try { room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session); } catch { }
            

            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            responseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            ////do we understand the room object to proceed if it exists
            //if (room != null)
            //{
            //    if (!AlexaSessionManager.Instance.ValidateRoomConfiguration(room, config))
            //        return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager,  userManager);

            //}
            ////if the room object doesn't exist in the request - we need one for devices which do not support APL
            //else
            //{
            if (room is null && session.alexaSessionDisplayType == AlexaSessionDisplayType.NONE)
                    return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager );
            //}

            //Clean up search term
            searchName = StringNormalization.NormalizeText(searchName);
            
            if (string.IsNullOrEmpty(searchName))
            {
                return new NotUnderstood().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager);
            }

            var result = EmbyControllerUtility.Instance.NarrowSearchResults(searchName, new[] { type }, session.User);

            // No result, No exist
            if (result is null)
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, session),
                    }
                });
            }

            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(session.User))
                return responseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    }
                });


            // user requested an Emby client/room display not this viewport
            // or user has designated a room from a prior request - display both if possible
            if (room != null)
                try { EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, result); } catch { }

            ServerEntryPoint.Instance.Log.Info("ALEXA ABOUT TO BUILD RENDER DOCUMENT");
            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {
                baseItems = new List<BaseItem>() { result },
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            //Update Session
            session.NowViewingBaseItem = result;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);
            
            try
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_ITEM, session, new List<BaseItem> { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                    },
                    person = session.person,
                    shouldEndSession = null,
                    directives = new List<Directive>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                    }

                }, session.alexaSessionDisplayType);

            }
            catch (Exception exception)
            {
                return ErrorHandler.Instance.OnError(exception.InnerException, alexaRequest, session, responseClient);
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BrowseBaseItemDetailsIntent : IIntentResponse
    {
        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {

            Room room = null;
            try { room = alexa.RoomContextManager.ValidateRoom(alexaRequest, session); } catch { }
            var displayNone = Equals(session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return alexa.RoomContextManager.RequestRoom(alexaRequest, session, alexa.ResponseClient);
            

            var request        = alexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = request.intent.slots.Movie.value is null ? "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            alexa.ResponseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            //Clean up search term
            searchName = StringNormalization.NormalizeText(searchName);

            if (string.IsNullOrEmpty(searchName)) return new NotUnderstood().Response(alexaRequest, session, alexa); //responseClient, libraryManager, sessionManager, userManager, roomContextManager);

            var result = EmbyControllerUtility.Instance.QuerySpeechResultItems(searchName, new[] { type }, session.User);

            if (result is null)
            {
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, session),
                    }
                });
            }
            
            if (!result.IsParentalAllowed(session.User))
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    }
                });

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, result);
                }
                catch (Exception exception)
                {
                    alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            Task.Delay(1500);

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
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_ITEM, session, new List<BaseItem> { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                    },
                    person = session.person,
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                    }

                }, session.alexaSessionDisplayType);

            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(exception.InnerException, alexaRequest, session, alexa.ResponseClient);
            }
        }
    }
}
using System;
using System.Collections.Generic;
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


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BrowseNextUpEpisodeIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, IAlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var roomManager = new RoomContextManager();
            Room room = null;
            try { room = roomManager.ValidateRoom(alexaRequest, session); } catch { }
            var displayNone = Equals(session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone)
                return roomManager.RequestRoom(alexaRequest, session, responseClient);

            var request = alexaRequest.request;
            var context = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";

            responseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);
            
            var nextUpEpisode = EmbyControllerUtility.Instance.GetNextUpEpisode(request.intent, session.User);
            
            if (nextUpEpisode is null)
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE, session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                        sound          = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    directives       = new List<Directive>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            HeadlinePrimaryText = "There doesn't seem to be a new episode available.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, session)
                    }
                }, session.alexaSessionDisplayType);
            }
            
            //Parental Control check for baseItem
            if (!nextUpEpisode.IsParentalAllowed(session.User))
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, session, new List<BaseItem>(){nextUpEpisode}),
                        sound     = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"

                    },
                    shouldEndSession = true
                });
            }

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, nextUpEpisode);
                }
                catch (Exception exception)
                {
                    responseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {
                baseItems          = new List<BaseItem>() {nextUpEpisode},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            session.NowViewingBaseItem = nextUpEpisode;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

            return responseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_NEXT_UP_EPISODE, session , new List<BaseItem>() {nextUpEpisode}),
                    sound          = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
                directives       = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                }

            }, session.alexaSessionDisplayType);
           
        }
    }
}

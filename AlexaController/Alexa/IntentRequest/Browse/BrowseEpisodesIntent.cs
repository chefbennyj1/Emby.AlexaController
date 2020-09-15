using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.IntentRequest.Rooms;
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
    public class BrowseEpisodesIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {

            var request        = alexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            //var room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);//(intent.slots.Room.value ?? session.room) ?? string.Empty;
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

            responseClient.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);

            ////do we understand the room object to proceed if it exists
            //if (!string.IsNullOrEmpty(room))
            //{
            //    if (!AlexaSessionManager.Instance.ValidateRoomConfiguration(room, Plugin.Instance.Configuration))
            //        return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager);
            //}
            ////if the room object doesn't exist do we need one
            //else
            //{
                var displayNone = session.alexaSessionDisplayType == AlexaSessionDisplayType.NONE;
                if (room == null && displayNone)
                    return new RoomContextIntent().Response(alexaRequest, session, responseClient, libraryManager, sessionManager, userManager);
            //}


            // This is a recursive request, so if the user is viewing media at "Series" or "Season" level
            // it will return the episode list for the season. ASK: "Show Season 1" / "Season 1" .
            var result = libraryManager.GetItemsResult(new InternalItemsQuery(session.User)
            {
                Parent            = session.NowViewingBaseItem,
                IncludeItemTypes  = new[] { "Episode" },
                ParentIndexNumber = Convert.ToInt32(seasonNumber),
                Recursive         = true
            });

            // User requested season/episode data that doesn't exist
            if (!result.Items.Any())
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NO_SEASON_ITEM_EXIST, session, null, new[] {seasonNumber}),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                    shouldEndSession = null,
                    person           = null,
                }, session.alexaSessionDisplayType);
            }

            var season = libraryManager.GetItemById(result.Items[0].Parent.InternalId);

            //if the user has requested an Emby client/room display during the session - display both if possible
            if (room != null)
                try { EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, season); } catch { }


            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {

                baseItems          = result.Items.ToList(),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = $"Season {seasonNumber}"
            };

            session.NowViewingBaseItem = season;
            session.room = room; 
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

            return responseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Season { seasonNumber}"
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

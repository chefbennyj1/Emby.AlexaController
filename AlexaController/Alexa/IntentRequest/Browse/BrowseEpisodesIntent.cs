using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class BrowseEpisodesIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public BrowseEpisodesIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
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
            
            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            Alexa.ResponseClient.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);
            
            // This is a recursive request, so if the user is viewing media at "Series" or "Season" level
            // it will return the episode list for the season. ASK: "Show Season 1" / "Season 1" .
            var result = Alexa.LibraryManager.GetItemsResult(new InternalItemsQuery(Session.User)
            {
                Parent            = Session.NowViewingBaseItem,
                IncludeItemTypes  = new[] { "Episode" },
                ParentIndexNumber = Convert.ToInt32(seasonNumber),
                Recursive         = true
            });

            // User requested season/episode data that doesn't exist
            if (!result.Items.Any())
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NO_SEASON_ITEM_EXIST, Session, null, new[] {seasonNumber}),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                    shouldEndSession = null,
                    person           = null,
                }, Session.alexaSessionDisplayType);
            }

            var season = Alexa.LibraryManager.GetItemById(result.Items[0].Parent.InternalId);

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, Session.User, season);
                }
                catch (Exception exception)
                {
                    Alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = result.Items.ToList(),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = $"Season {seasonNumber}"
            };

            Session.NowViewingBaseItem = season;
            Session.room = room; 
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Season { seasonNumber}"
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                }
            }, Session.alexaSessionDisplayType);

        }
    }
}

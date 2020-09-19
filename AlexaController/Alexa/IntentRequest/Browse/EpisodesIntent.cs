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
using MediaBrowser.Model.Entities;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class EpisodesIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public EpisodesIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            ;
            Session = session;
            ;
        }

        public string Response()
        {     
            Room room = null;
            try { room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return RoomContextManager.Instance.RequestRoom(AlexaRequest, Session, ResponseClient.Instance);
            
            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            ResponseClient.Instance.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);


            var result = EmbyServerEntryPoint.Instance.GetEpisodes(Convert.ToInt32(seasonNumber),
                Session.NowViewingBaseItem, Session.User);
            
            // User requested season/episode data that doesn't exist
            if (!result.Any())
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
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

            var seasonId = result[0].Parent.InternalId;
            var season = EmbyServerEntryPoint.Instance.GetItemById(seasonId);

            if (!(room is null))
                try
                {
                    EmbyServerEntryPoint.Instance.BrowseItemAsync(room.Name, Session.User, season);
                }
                catch (Exception exception)
                {
                    ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = result,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = $"Season {seasonNumber}",
                HeaderAttributionImage = season.Parent.HasImage(ImageType.Logo) ? $"/Items/{season.Parent.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            Session.NowViewingBaseItem = season;
            Session.room = room; 
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
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

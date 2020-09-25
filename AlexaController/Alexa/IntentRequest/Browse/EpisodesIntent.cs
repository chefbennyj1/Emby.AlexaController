using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
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

        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);


            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;


            ResponseClient.Instance.PostProgressiveResponse($"{SpeechSemantics.SpeechResponse(SpeechType.COMPLIANCE)} {SpeechSemantics.SpeechResponse(SpeechType.REPOSE)}", apiAccessToken, requestId);


            var result = EmbyServerEntryPoint.Instance.GetEpisodes(Convert.ToInt32(seasonNumber),
                Session.NowViewingBaseItem, Session.User);
            
            // User requested season/episode data that doesn't exist
            if (!result.Any())
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.NO_SEASON_ITEM_EXIST, Session, null, new[] {seasonNumber}),
                        speechType = SpeechType.APOLOGETIC,
                    },
                    shouldEndSession = null,
                    person           = null,
                }, Session.alexaSessionDisplayType).Result;
            }

            var seasonId = result[0].Parent.InternalId;
            var season = EmbyServerEntryPoint.Instance.GetItemById(seasonId);

            if (!(Session.room is null))
                try
                {
                    EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, season);
                }
                catch (Exception exception)
                {
                    ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    Session.room = null;
                }

            Task.Delay(1200);

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = result,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = $"Season {seasonNumber}",
                HeaderAttributionImage = season.Parent.HasImage(ImageType.Logo) ? $"/Items/{season.Parent.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            Session.NowViewingBaseItem = season;
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
            }, Session.alexaSessionDisplayType).Result;

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class EpisodesIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public EpisodesIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            var progressiveSpeech = SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = Session
            });

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            
            var result = EmbyServerEntryPoint.Instance.GetEpisodes(Convert.ToInt32(seasonNumber),
                Session.NowViewingBaseItem, Session.User);
            
            // User requested season/episode data that doesn't exist
            if (!result.Any())
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(new SpeechStringQuery()
                        {
                            type = SpeechResponseType.NO_SEASON_ITEM_EXIST, 
                            session = Session, 
                            args = new[] {seasonNumber}
                        }),
                        
                    },
                    shouldEndSession = null,
                    person           = null,
                }, Session.alexaSessionDisplayType);
            }

            var seasonId = result[0].Parent.InternalId;
            var season = EmbyServerEntryPoint.Instance.GetItemById(seasonId);

            if (!(Session.room is null))
            {
                try
                {
                    await EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, season);
                }
                catch (Exception exception)
                {
                    await Task.Run(() =>
                            ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems              = result,
                renderDocumentType     = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle            = $"Season {seasonNumber}",
                HeaderAttributionImage = season.Parent.HasImage(ImageType.Logo) ? $"/Items/{season.Parent.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            Session.NowViewingBaseItem = season;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Season { seasonNumber}"
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, Session.alexaSessionDisplayType);

        }
    }
}

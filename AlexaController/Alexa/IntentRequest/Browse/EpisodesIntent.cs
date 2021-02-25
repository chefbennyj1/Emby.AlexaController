using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Model.RequestData;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.APLA.AudioFilters;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;
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

            
            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = Session
            //});

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse("One moment please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            
            var results = ServerQuery.Instance.GetEpisodes(Convert.ToInt32(seasonNumber),
                Session.NowViewingBaseItem, Session.User);

            InternalRenderAudioQuery renderAudioTemplateInfo = null;
            

            // User requested season/episode data that doesn't exist
            if (!results.Any())
            {
                renderAudioTemplateInfo = new InternalRenderAudioQuery()
                {
                    speechContent = SpeechContent.NO_SEASON_ITEM_EXIST,
                    session = Session,
                    args = new[] {seasonNumber},
                    audio = new Audio()
                    {
                        source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                        filter = new List<IFilter>() { new Volume() { amount = 0.5} }
                    }
                };
                return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    //outputSpeech = new OutputSpeech()
                    //{
                    //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                    //    {
                    //        type = SpeechResponseType.NO_SEASON_ITEM_EXIST, 
                    //        session = Session, 
                    //        args = new[] {seasonNumber}
                    //    }),
                        
                    //},
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderAudioManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo)
                    }
                }, Session);
            }

            var seasonId = results[0].Parent.InternalId;
            var season = ServerQuery.Instance.GetItemById(seasonId);

            if (!(Session.room is null))
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, season);
                }
                catch (Exception exception)
                {
                    await Task.Run(() => ResponseClient.Instance
                            .PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            var documentTemplateInfo = new InternalRenderDocumentQuery()
            {
                baseItems              = results,
                renderDocumentType     = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle            = $"Season {seasonNumber}",
                HeaderAttributionImage = season.Parent.HasImage(ImageType.Logo) ? $"/Items/{season.Parent.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            renderAudioTemplateInfo = new InternalRenderAudioQuery()
            {
                speechContent = SpeechContent.BROWSE_ITEM,
                session = Session,
                items = new List<BaseItem>() { season },
                args = new[] {seasonNumber},
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };


            Session.NowViewingBaseItem = season;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
            var renderAudioDirective    = await RenderAudioManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo);

            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                SpeakUserName    = true,
                directives       = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);

        }
    }
}

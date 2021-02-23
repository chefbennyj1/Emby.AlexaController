using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class NextUpEpisodeIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session)
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

            var request           = AlexaRequest.request;
            var intent            = request.intent;
            var slots             = intent.slots;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = Session
            //});

#pragma warning disable 4014
            Task.Run( () => ResponseClient.Instance.PostProgressiveResponse("One moment Please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session.User);
            
            if (nextUpEpisode is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    //outputSpeech = new OutputSpeech()
                    //{
                    //    phrase         = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    //    {
                    //        type = SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE, 
                    //        session  =Session
                    //    }),
                    //    sound          = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    //},
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives       = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "There doesn't seem to be a new episode available.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session),
                        await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(new RenderAudioTemplate()
                        {
                            speechContent = SpeechContent.NO_NEXT_UP_EPISODE_AVAILABLE,
                            session = Session,
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        })
                    }
                }, Session);
            }
            
            //Parental Control check for baseItem
            if (!nextUpEpisode.IsParentalAllowed(Session.User))
            {
                if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                {
                    await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                        $"{Session.User} attempted to view a restricted item.", $"{Session.User} attempted to view {nextUpEpisode.Name}.").ConfigureAwait(false);
                }

                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    //outputSpeech = new OutputSpeech()
                    //{
                    //    phrase    = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    //    {
                    //        type = SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, 
                    //        session = Session, 
                    //        items = new List<BaseItem>(){nextUpEpisode}
                    //    }),
                    //    sound     = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"

                    //},
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(
                            new RenderDocumentTemplate()
                            {
                                renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                                HeadlinePrimaryText = $"Stop! Rated {nextUpEpisode.OfficialRating}"

                            }, Session),
                        await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(
                            new RenderAudioTemplate()
                            {
                                speechContent = SpeechContent.PARENTAL_CONTROL_NOT_ALLOWED,
                                session = Session,
                                items = new List<BaseItem>(){nextUpEpisode},
                                audio = new Audio()
                                {
                                    source = "soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02",
                                    
                                }
                            })
                    }
                }, Session);
            }

            if (!(Session.room is null))
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, nextUpEpisode);
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

            var series = nextUpEpisode.Parent.Parent;
            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = new List<BaseItem>() {nextUpEpisode},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                HeaderAttributionImage = series.HasImage(ImageType.Logo) ? $"/Items/{series.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            var audioTemplateInfo = new RenderAudioTemplate()
            {
                speechContent = SpeechContent.BROWSE_NEXT_UP_EPISODE,
                session = Session,
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
            var renderAudioDirective    = await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(audioTemplateInfo);
            
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                //    {
                //        type = SpeechResponseType.BROWSE_NEXT_UP_EPISODE, 
                //        session = Session , 
                //        items = new List<BaseItem>() {nextUpEpisode}
                //    }),
                //    sound  = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                //},
                shouldEndSession = null,
                SpeakUserName = true,
                directives       = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);
           
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;


namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class NextUpEpisodeIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
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

            IDataSource dataSource = null;
            
            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session.User);
            
            if (nextUpEpisode is null)
            {
                dataSource =
                    await DataSourceManager.Instance.GetGenericHeadline(
                        "There doesn't seem to be a new episode available.");
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session),
                        await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(new AudioDirectiveQuery()
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

                dataSource =
                    await DataSourceManager.Instance.GetGenericHeadline($"Stop! Rated {nextUpEpisode.OfficialRating}"); 

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session),
                        await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(
                            new AudioDirectiveQuery()
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
                            AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            //var series = nextUpEpisode.Parent.Parent;
            //var documentTemplateInfo = new RenderDocumentQuery()
            //{
            //    baseItems          = new List<BaseItem>() {nextUpEpisode},
            //    renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
            //    HeaderAttributionImage = series.HasImage(ImageType.Logo) ? $"/Items/{series.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            //};

            dataSource = await DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(nextUpEpisode, Session);

            var audioTemplateInfo = new AudioDirectiveQuery()
            {
                speechContent = SpeechContent.BROWSE_NEXT_UP_EPISODE,
                session = Session,
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, dataSource);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, Session);
            var renderAudioDirective    = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(audioTemplateInfo);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);
        }
    }
}

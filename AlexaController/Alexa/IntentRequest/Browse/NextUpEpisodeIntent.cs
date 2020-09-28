using System.Collections.Generic;
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

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request           = AlexaRequest.request;
            var context           = AlexaRequest.context;
            var apiAccessToken    = context.System.apiAccessToken;
            var requestId         = request.requestId;

            var progressiveSpeech = SpeechStrings.GetPhrase(SpeechResponseType.PROGRESSIVE_RESPONSE, Session);

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var nextUpEpisode = EmbyServerEntryPoint.Instance.GetNextUpEpisode(request.intent, Session.User);
            
            if (nextUpEpisode is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase         = SpeechStrings.GetPhrase(SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE, Session),
                        sound          = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    directives       = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "There doesn't seem to be a new episode available.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session)
                    }
                }, Session.alexaSessionDisplayType);
            }
            
            //Parental Control check for baseItem
            if (!nextUpEpisode.IsParentalAllowed(Session.User))
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>(){nextUpEpisode}),
                        sound     = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"

                    },
                    shouldEndSession = true
                });
            }

            if (!(Session.room is null))
                try
                {
#pragma warning disable 4014
                    Task.Run(() => EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, nextUpEpisode)).ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch (BrowseCommandException exception)
                {
#pragma warning disable 4014
                    Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
                    await Task.Delay(1200);
                    Session.room = null;
                }
            
            var series = nextUpEpisode.Parent.Parent;
            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = new List<BaseItem>() {nextUpEpisode},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                HeaderAttributionImage = series.HasImage(ImageType.Logo) ? $"/Items/{series.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.BROWSE_NEXT_UP_EPISODE, Session , new List<BaseItem>() {nextUpEpisode}),
                    sound  = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    await renderDocumentDirective
                }

            }, Session.alexaSessionDisplayType);
           
        }
    }
}

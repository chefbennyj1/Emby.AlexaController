using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#pragma warning disable 4014

// ReSharper disable TooManyChainedReferences
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class PlayItemIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; private set; }

        public PlayItemIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            if (Session.room is null) return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            var intent         = request.intent;
            var slots          = intent.slots;

            AlexaResponseClient.Instance.PostProgressiveResponse(SpeechBuilderService.GetSpeechPrefix(SpeechPrefix.REPOSE) + " Starting Playback.",apiAccessToken, requestId);

            BaseItem result;
            if (Session.NowViewingBaseItem is null)
            {
                var type = slots.Movie.value is null ? "Series" : "Movie";
                result = ServerQuery.Instance.QuerySpeechResultItem(type == "Movie" ? slots.Movie.value : slots.Series.value, new[] { type });
            }
            else
            {
                result = Session.NowViewingBaseItem;
            }

            //Item doesn't exist in the library
            if (result is null)
            {
                var aplaDataSource = await DataSourceAudioSpeechPropertiesManager.Instance.NoItemExists();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(Session.User))
            {
                if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                {
                    await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                        $"{Session.User} attempted to view a restricted item.", $"{Session.User} attempted to view {result.Name}.").ConfigureAwait(false);
                }

                var genericLayoutProperties =
                    await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync($"Stop! Rated {result.OfficialRating}", "/particles");

                var parentalControlNotAllowedAudioSpeech = await DataSourceAudioSpeechPropertiesManager.Instance.ParentalControlNotAllowed(result, Session);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(genericLayoutProperties, Session),
                        await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(parentalControlNotAllowedAudioSpeech)

                    }

                }, Session);
            }
            
            try
            {

                ServerController.Instance.PlayMediaItemAsync(Session, result);

            }
            catch (Exception exception)
            {
               AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
            }

            Session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            var detailLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(result, Session);

            var playItemAudioSpeech = await DataSourceAudioSpeechPropertiesManager.Instance.PlayItem(result);

            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(detailLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(playItemAudioSpeech);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {

                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, Session);

        }
    }
}

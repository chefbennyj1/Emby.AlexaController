using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;

// ReSharper disable TooManyChainedReferences
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayItemIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        //If no room is requested in the PlayItemIntent intent, we follow up immediately to get a room value from 'RoomName' intent. 

        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        
        public PlayItemIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }

        public async Task<string> Response()
        {
            try { Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
            if (Session.room is null) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            var intent         = request.intent;
            var slots          = intent.slots;

            IDataSource aplDataSource = null;
            IDataSource aplaDataSource = null;

            BaseItem result = null;
            if (Session.NowViewingBaseItem is null)
            {
                var type = slots.Movie.value is null ? "Series" : "Movie";
                result = ServerQuery.Instance.QuerySpeechResultItem(
                    type == "Movie" ? slots.Movie.value : slots.Series.value, new[] { type });
            }
            else
            {
                result = Session.NowViewingBaseItem;
            }
            
            //Item doesn't exist in the library
            if (result is null)
            {
                aplaDataSource = await AplaDataSourceManager.Instance.GenericItemDoesNotExists();
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
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

                aplDataSource =
                    await AplDataSourceManager.Instance.GetGenericHeadline($"Stop! Rated {result.OfficialRating}");

                aplaDataSource = await AplaDataSourceManager.Instance.ParentalControlNotAllowed(result, Session);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(aplDataSource, Session),
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    
                    }
                 
                }, Session);
            }

            try
            {
#pragma warning disable 4014
                await ServerController.Instance.PlayMediaItemAsync(Session, result);
#pragma warning restore 4014
            }
            catch (Exception exception)
            {
#pragma warning disable 4014
                Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
                await Task.Delay(1200);
            }

            Session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            aplDataSource = await AplDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(result, Session);

            aplaDataSource = await AplaDataSourceManager.Instance.PlayItem(result);
           
            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(aplDataSource, Session);
            var renderAudioDirective    = await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                SpeakUserName = true,
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

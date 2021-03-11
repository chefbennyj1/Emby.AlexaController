using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceProperties;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Logging;

namespace AlexaController.Alexa.IntentRequest.Browse
{
  
    public class BaseItemDetailsByNameIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public BaseItemDetailsByNameIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
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
            var type           = slots.Movie.value is null ? slots.Series.value is null ? "" : "Series" : "Movie";
            var searchName     = (string)(slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            IDataSource aplDataSource = null;
            IDataSource aplaDataSource = null;

            ServerController.Instance.Log.Info(searchName);
            
#pragma warning disable 4014
            Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse("One moment Please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            
            //Clean up search term
            searchName = StringNormalization.ValidateSpeechQueryString(searchName);

            if (string.IsNullOrEmpty(searchName)) return await new NotUnderstood(AlexaRequest, Session).Response();
            
            var result = ServerQuery.Instance.QuerySpeechResultItem(searchName, new[] { type });
            
            if (result is null)
            {
                aplaDataSource = await AplaDataSourceManager.Instance.NoItemExists(Session, searchName);
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            //User should not access this item. Warn the user, and place a notification in the Emby Activity Label
            if (!result.IsParentalAllowed(Session.User))
            {
                try
                {
                    if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                    {
                        await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                            $"{Session.User} attempted to view a restricted item.",
                            $"{Session.User} attempted to view {result.Name}.");
                    }
                }
                catch { }

                aplDataSource = await AplDataSourceManager.Instance.GetGenericViewDataSource($"Stop! Rated {result.OfficialRating}", "/particles");

                aplaDataSource = await AplaDataSourceManager.Instance.ParentalControlNotAllowed(result, Session);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session),
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }
            

            if (!(Session.room is null))
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, result);
                }
                catch (Exception exception)
                {
#pragma warning disable 4014
                    Task.Run(() =>
                            AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            aplDataSource = await AplDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(result, Session);

            aplaDataSource = await AplaDataSourceManager.Instance.ItemBrowse(result, Session);

            //Update Session
            Session.NowViewingBaseItem = result;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session);
            var renderAudioDirective    = await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);

            try
            {
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
            catch (Exception exception)
            {
                throw new Exception("I was unable to build the render document. " + exception.Message);
            }
        }
    }
}
using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Model.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.EmbyApl;

#pragma warning disable 4014

namespace AlexaController.Api.IntentRequest.Browse
{
    [Intent]
    // ReSharper disable once UnusedType.Global
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
            AlexaResponseClient.Instance.PostProgressiveResponse(SpeechBuilderService.GetSpeechPrefix(SpeechPrefix.REPOSE),
                AlexaRequest.context.System.apiAccessToken, AlexaRequest.request.requestId);

            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request    = AlexaRequest.request;
            var intent     = request.intent;
            var slots      = intent.slots;
            var type       = slots.Movie.value is null ? slots.Series.value is null ? "" : "Series" : "Movie";
            
            var searchName = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;

            //Clean up search term
            searchName = StringNormalization.ValidateSpeechQueryString(searchName);

            if (string.IsNullOrEmpty(searchName)) return await new NotUnderstood(AlexaRequest, Session).Response();

            var result = ServerQuery.Instance.QuerySpeechResultItem(searchName, new[] { type });

            
            if (result is null)
            {
                var aplaDataSourceProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                {
                    SpeechResponseType = SpeechResponseType.NoItemExists
                });
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSourceProperties)
                    }
                }, Session);
            }

            //User should not access this item. Warn the user, and place a notification in the Emby Activity Label
            if (!result.IsParentalAllowed(Session.User))
            {
                try
                {
                    var config = Plugin.Instance.Configuration;
                    if (config.EnableServerActivityLogNotifications)
                    {
                        await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                            $"{Session.User} attempted to view a restricted item.",
                            $"{Session.User} attempted to view {result.Name}.");
                    }
                }
                catch { }

                var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync($"Stop! Rated {result.OfficialRating}", "/particles");
                var parentalControlNotAllowedAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                {
                    SpeechResponseType = SpeechResponseType.ParentalControlNotAllowed,
                    item = result,
                    session = Session
                });

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, Session),
                        await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(parentalControlNotAllowedAudioProperties)
                    }
                }, Session);
            }

            if (Session.hasRoom)
            {
                try
                {
                    ServerController.Instance.BrowseItemAsync(Session, result);
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            var detailLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(result, Session);
            var aplaDataSource1 = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.ItemBrowse,
                item = result,
                session = Session
            }
            );

            //Update Session
            Session.NowViewingBaseItem = result;
            AlexaSessionManager.Instance.UpdateSession(Session, detailLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(detailLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource1);

            try
            {
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
            catch (Exception exception)
            {
                throw new Exception("I was unable to build the render document. " + exception.Message);
            }
        }
    }
}
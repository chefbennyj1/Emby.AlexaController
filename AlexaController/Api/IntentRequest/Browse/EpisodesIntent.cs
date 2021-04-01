﻿using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.EmbyApl;

namespace AlexaController.Api.IntentRequest.Browse
{
    [Intent]
    public class EpisodesIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public EpisodesIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var seasonNumber = slots.SeasonNumber.value;
            var context = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var results = ServerQuery.Instance.GetEpisodes(Convert.ToInt32(seasonNumber), Session.NowViewingBaseItem, Session.User);

            // User requested season/episode data that doesn't exist
            if (!results.Any())
            {
                var aplaDataSource = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
                {
                    SpeechResponseType = SpeechResponseType.NoItemExists
                });

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource)
                    }
                }, Session);
            }

            var seasonId = results[0].Parent.InternalId;
            var season = ServerQuery.Instance.GetItemById(seasonId);

            if (Session.hasRoom)
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, season);
                }
                catch (Exception exception)
                {
                    await Task.Run(() => AlexaResponseClient.Instance
                            .PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            var sequenceLayoutProperties = await DataSourcePropertiesManager.Instance.GetSequenceViewPropertiesAsync(results, season.Parent);
            var aplaDataSource1 = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.ItemBrowse,
                item = season,
                session = Session
            });

            Session.NowViewingBaseItem = season;
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(sequenceLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource1);

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
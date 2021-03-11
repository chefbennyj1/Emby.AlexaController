﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.AudioFilters;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceProperties;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;


namespace AlexaController.Alexa.IntentRequest.Browse
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
            
            var results = ServerQuery.Instance.GetEpisodes(Convert.ToInt32(seasonNumber),
                Session.NowViewingBaseItem, Session.User);
            

            IDataSource aplaDataSource = null;

            // User requested season/episode data that doesn't exist
            if (!results.Any())
            {
                aplaDataSource = await AplaDataSourceManager.Instance.NoItemExists(Session, seasonNumber);
                
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource)
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
                    await Task.Run(() => AlexaResponseClient.Instance
                            .PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }
            
            var aplDataSource = await AplDataSourceManager.Instance.GetSequenceItemsDataSourceAsync(results, season.Parent);
            aplaDataSource = await AplaDataSourceManager.Instance.ItemBrowse(season, Session);
           
            Session.NowViewingBaseItem = season;
            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session);
            var renderAudioDirective    = await RenderAudioDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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

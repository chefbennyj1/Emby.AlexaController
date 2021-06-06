using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities.TV;

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
            try
            {
                Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            Session.hasRoom = !(Session.room is null);
            
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var seasonNumber   = slots.SeasonNumber.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            ServerController.Instance.Log.Info($"Preparing Episode Results...");
            var episodes = ServerDataQuery.Instance.GetEpisodes(Convert.ToInt32(seasonNumber), Session.NowViewingBaseItem, Session.User);

            // User requested season/episode data that doesn't exist
            if (!episodes.Any())
            {
                var aplaDataSource = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
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

            var seasonId = episodes[0].Parent.InternalId; //Get the first item in the baseItem collection (an episode) and look at it's parent for the Season BaseItem
            var season = ServerDataQuery.Instance.GetItemById(seasonId); //Season Data
            ServerController.Instance.Log.Info($"Season {season.Name} data found.");
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

            var sequenceViewProperties = await DataSourcePropertiesManager.Instance.GetBaseItemCollectionSequenceViewPropertiesAsync(episodes, season.Parent);
            
            var aplAudioResponse = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.ItemBrowse,
                item = season,
                session = Session
            });

            Session.NowViewingBaseItem = season;
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceViewProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(sequenceViewProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplAudioResponse);

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

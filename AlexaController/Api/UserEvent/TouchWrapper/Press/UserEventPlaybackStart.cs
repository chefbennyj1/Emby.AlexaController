using AlexaController.Alexa;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#pragma warning disable 4014


namespace AlexaController.Api.UserEvent.TouchWrapper.Press
{
    /// <summary>
    /// request arguments[0] will be "UserEventPlaybackStart"
    /// request arguments[1] will be the room name. This can not be null.
    /// </summary>

    public class UserEventPlaybackStart : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }


        public UserEventPlaybackStart(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }

        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var source = request.source;
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = ServerDataQuery.Instance.GetItemById(source.id);

            session.room = session.room ?? RoomContextManager.Instance.GetRoomByName(request.arguments[1]);

            if (session.room is null)
            {
                var roomSelectionViewProperties = await DataSourcePropertiesManager.Instance.GetRoomSelectionViewPropertiesAsync(baseItem, session);
                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, roomSelectionViewProperties);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(roomSelectionViewProperties, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);
            var startPosition = request.arguments[2] is null ? 0 : Convert.ToInt64(request.arguments[2]);

            ServerController.Instance.PlayMediaItemAsync(session, baseItem, startPosition);

            var detailLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(baseItem, session);
            var playItemAudioSpeech = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.PlayItem,
                item = baseItem
            });

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(detailLayoutProperties, session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(playItemAudioSpeech);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, session);

        }
    }
}

using System;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
#pragma warning disable 4014


namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
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
            var baseItem = ServerQuery.Instance.GetItemById(source.id);

            session.room = session.room ?? RoomContextManager.Instance.GetRoomByName(request.arguments[1]);

            if (session.room is null)
            {
                var roomSelectionViewProperties = await DataSourceLayoutPropertiesManager.Instance.GetRoomSelectionViewPropertiesAsync(baseItem, session);
                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, roomSelectionViewProperties);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(roomSelectionViewProperties, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);
            var startPosition = request.arguments[2] is null ? 0 : Convert.ToInt64(request.arguments[2]);
            
            ServerController.Instance.PlayMediaItemAsync(session, baseItem, startPosition);
            
            var detailLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(baseItem, session);
            var playItemAudioSpeech = await DataSourceAudioSpeechPropertiesManager.Instance.PlayItem(baseItem);
            
            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(detailLayoutProperties, session);
            var renderAudioDirective = await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(playItemAudioSpeech);

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

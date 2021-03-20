using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    /// <summary>
    /// request arguments[0] will be "UserEventPlaybackStart"
    /// request arguments[1] will be the room name
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

            //IDataSource aplDataSource = null;
            //IDataSource aplaDataSource = null;


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
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(roomSelectionViewProperties, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014


            var detailLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetBaseItemDetailViewPropertiesAsync(baseItem, session);
            var aplaDataSource = await DataSourceAudioSpeechPropertiesManager.Instance.PlayItem(baseItem);


            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(detailLayoutProperties, session);
            var renderAudioDirective = await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource);

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

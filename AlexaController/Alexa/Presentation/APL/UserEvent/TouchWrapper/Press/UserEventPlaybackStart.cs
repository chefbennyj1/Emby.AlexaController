﻿using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;


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

            session.room = session.room ?? RoomManager.Instance.GetRoomByName(request.arguments[1]);

            IDataSource aplDataSource = null;
            IDataSource aplaDataSource = null;


            if (session.room is null)
            {
                aplDataSource = await AplObjectDataSourceManager.Instance.GetRoomSelection(baseItem, session);
                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, aplDataSource);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014


            aplDataSource = await AplObjectDataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(baseItem, session);
            aplaDataSource = await AplAudioDataSourceManager.Instance.PlayItem(baseItem);


            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, session);
            var renderAudioDirective = await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                SpeakUserName = true,
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

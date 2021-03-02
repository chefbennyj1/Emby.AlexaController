using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;


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
            var request  = AlexaRequest.request;
            var source   = request.source;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = ServerQuery.Instance.GetItemById(source.id);

            session.room = session.room ?? RoomManager.Instance.GetRoomByName(request.arguments[1]);

            IDataSource dataSource = null;

            //RenderDocumentQuery documentTemplateInfo = null;
            AudioDirectiveQuery audioTemplateInfo = null;
            if (session.room is null)
            {
                //documentTemplateInfo = new RenderDocumentQuery()
                //{
                //    renderDocumentType = RenderDocumentType.ROOM_SELECTION_TEMPLATE,
                //    baseItems = new List<BaseItem>() {baseItem}
                //};
                dataSource = await DataSourceManager.Instance.GetRoomSelection(baseItem, session);
                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, dataSource);

                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014

            //documentTemplateInfo = new RenderDocumentQuery()
            //{
            //    baseItems = new List<BaseItem>() {baseItem},
            //    renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            //};

            dataSource = await DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(baseItem, session);

            audioTemplateInfo = new AudioDirectiveQuery()
            {
                speechContent = SpeechContent.PLAY_MEDIA_ITEM,
                session = session, 
                items = new List<BaseItem>() { baseItem },
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, session);
            var renderAudioDirective = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(audioTemplateInfo);

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

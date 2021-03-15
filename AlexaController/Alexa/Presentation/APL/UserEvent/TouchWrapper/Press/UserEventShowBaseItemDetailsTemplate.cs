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
    public class UserEventShowBaseItemDetailsTemplate : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public UserEventShowBaseItemDetailsTemplate(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            ServerController.Instance.Log.Info("UserEventShowBaseItemDetailsTemplate");
            var request = AlexaRequest.request;
            var source = request.source;
            var baseItem = ServerQuery.Instance.GetItemById(source.id);
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var room = session.room;

            var aplDataSource = await APL_DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(baseItem, session);
            var aplaDataSource = await APLA_DataSourceManager.Instance.ItemBrowse(baseItem, session);

            // Update session data
            session.NowViewingBaseItem = baseItem;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session, aplDataSource);

            //if the user has requested an Emby client/room display during the session - display both if possible
            if (!(room is null))
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => ServerController.Instance.BrowseItemAsync(session, baseItem))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch
                {
                }
            }

            var renderDocumentDirective = await APL_RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, session);
            var renderAudioDirective = await APLA_RenderDocumentManager.Instance.GetAudioDirectiveAsync(aplaDataSource);

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

using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventShowItemListSequenceTemplate : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public UserEventShowItemListSequenceTemplate(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }

        public async Task<string> Response()
        {
            ServerController.Instance.Log.Info($"UserEventShowItemListSequenceTemplate");
            var request = AlexaRequest.request;
            var source = request.source;
            var baseItem = ServerQuery.Instance.GetItemById(source.id);
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var type = baseItem.GetType().Name;

            var results = ServerQuery.Instance.GetItemsResult(baseItem,
                new[] { type == "Series" ? "Season" : "Episode" }, session.User);


            var dataSource =
                await AplObjectDataSourceManager.Instance.GetSequenceItemsDataSourceAsync(results.Items.ToList(), baseItem);

            session.NowViewingBaseItem = baseItem;
            AlexaSessionManager.Instance.UpdateSession(session, dataSource);

            //if the user has requested an Emby client/room display during the session - display both if possible
            if (session.room != null)
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

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(dataSource, session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = ""
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, session);

        }
    }
}

using AlexaController.Alexa;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Api.UserEvent.TouchWrapper.Press
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
            var request = AlexaRequest.request;
            var source = request.source;
            var baseItem = ServerDataQuery.Instance.GetItemById(source.id);
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var type = baseItem.GetType().Name;

            var results = ServerDataQuery.Instance.GetItemsResult(baseItem,
                new[] { type == "Series" ? "Season" : "Episode" }, session.User);

            var sequenceViewProperties =
                await DataSourcePropertiesManager.Instance.GetBaseItemCollectionSequenceViewPropertiesAsync(results.Items.ToList(), baseItem);

            session.NowViewingBaseItem = baseItem;
            AlexaSessionManager.Instance.UpdateSession(session, sequenceViewProperties);

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

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(sequenceViewProperties, session);

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

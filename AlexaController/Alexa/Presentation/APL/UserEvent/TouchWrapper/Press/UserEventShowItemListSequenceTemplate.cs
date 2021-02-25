using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;


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
            var request  = AlexaRequest.request;
            var source   = request.source;
            var baseItem = ServerQuery.Instance.GetItemById(source.id);
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var type     = baseItem.GetType().Name;
           
           
            var results = ServerQuery.Instance.GetItemsResult(baseItem,
                new[] { type == "Series" ? "Season" : "Episode" }, session.User);

            var documentTemplateInfo = new InternalRenderDocumentQuery()
            {
                baseItems          = results.Items.ToList(),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = type == "Season" ? $"{ baseItem.Parent.Name } > { baseItem.Name }" : baseItem.Name
            };
           
            session.NowViewingBaseItem = baseItem;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);
            
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

            var renderDocumentDirective = await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);
            
            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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

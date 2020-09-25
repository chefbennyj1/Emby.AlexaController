using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
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
            var request  = AlexaRequest.request;
            var source   = request.source;
            var baseItem = EmbyServerEntryPoint.Instance.GetItemById(source.id);
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var type     = baseItem.GetType().Name;
           
            var phrase = "";
            
            var results = EmbyServerEntryPoint.Instance.GetBaseItems(baseItem,
                new[] {type == "Series" ? "Season" : "Episode"}, session.User);

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = results,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = type == "Season" ? $"{baseItem.Parent.Name} > {baseItem.Name}" : baseItem.Name
            };
           
            session.NowViewingBaseItem = baseItem;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);


            //if the user has requested an Emby client/room display during the session - display both if possible
            if (session.room != null)
                try { EmbyServerEntryPoint.Instance.BrowseItemAsync(session, baseItem); } catch { }
            

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = phrase
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                }
            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);

        }
    }
}

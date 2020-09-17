using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventShowItemListSequenceTemplate : IUserEventResponse
    {
        public string Response(IAlexaRequest alexaRequest, AlexaEntryPoint alexa)
        //(IAlexaRequest alexaRequest, ILibraryManager libraryManager, IResponseClient responseClient, ISessionManager sessionManager)
        {
            var request  = alexaRequest.request;
            var source   = request.source;
            var baseItem = alexa.LibraryManager.GetItemById(source.id);
            var session  = AlexaSessionManager.Instance.GetSession(alexaRequest);
            var room     = session.room;
            var type     = baseItem.GetType().Name;

           
            var phrase = "";
            
            var result   = alexa.LibraryManager.GetItemsResult(new InternalItemsQuery(session.User)
            {
                Parent           =  baseItem,
                IncludeItemTypes = new [] { type == "Series" ? "Season" : "Episode" },
                Recursive        = true
            });


            var documentTemplateInfo = new RenderDocumentTemplateInfo()
            {
                baseItems          = result.Items.ToList(),
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle        = type == "Season" ? $"{baseItem.Parent.Name} > {baseItem.Name}" : baseItem.Name
            };
            
            // Update session data
            session.NowViewingBaseItem = baseItem;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);


            //if the user has requested an Emby client/room display during the session - display both if possible
            if (room != null)
                try { EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, session.User, baseItem); } catch { }
            

            return alexa.ResponseClient.BuildAlexaResponse(new Response()
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

using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class goBack : IUserEventResponse
    {
        public string Response(IAlexaRequest alexaRequest, AlexaEntryPoint alexa)
        //(IAlexaRequest alexaRequest, ILibraryManager libraryManager, IResponseClient responseClient, ISessionManager sessionManager)
        {
            var session = AlexaSessionManager.Instance.GetSession(alexaRequest);

            var previousPage = session.paging.pages[session.paging.currentPage - 1];
            var currentPage = session.paging.pages[session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(session, currentPage, true);

            //if the user has requested an Emby client/room display during the session - go back on both if possible
            if (session.room != null)
                try { EmbyControllerUtility.Instance.BrowseItemAsync(session.room.Name, session.User, alexa.LibraryManager.GetItemById(session.NowViewingBaseItem.Parent.InternalId)); } catch { }

            return alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(previousPage, session)
                }

            }, session.alexaSessionDisplayType);
        }
    }
}
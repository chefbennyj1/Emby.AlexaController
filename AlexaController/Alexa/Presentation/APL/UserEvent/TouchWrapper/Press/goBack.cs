using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;

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
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaEntryPoint Alexa { get; }

        public goBack(IAlexaRequest alexaRequest, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
        }
        public string Response()
        //(IAlexaRequest alexaRequest, ILibraryManager libraryManager, IResponseClient responseClient, ISessionManager sessionManager)
        {
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);

            var previousPage = session.paging.pages[session.paging.currentPage - 1];
            var currentPage = session.paging.pages[session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(session, currentPage, true);

            //if the user has requested an Emby client/room display during the session - go back on both if possible
            if (session.room != null)
                try { EmbyControllerUtility.Instance.BrowseItemAsync(session.room.Name, session.User, Alexa.LibraryManager.GetItemById(previousPage.baseItems[0].InternalId)); } catch { }

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
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
// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class PreviousIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public PreviousIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            ;
            Session = session;
            ;
        }
        public string Response()
        {
            
            var previousPage = Session.paging.pages[Session.paging.currentPage - 1];
            var currentPage  = Session.paging.pages[Session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(Session, currentPage, true);

            //if the user has requested an Emby client/room display during the session - go back on both if possible
            if (Session.room != null)
                try { EmbyServerEntryPoint.Instance.BrowseItemAsync(Session,EmbyServerEntryPoint.Instance.GetItemById(Session.NowViewingBaseItem.Parent.InternalId)); } catch { }

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(previousPage, Session)
                }

            }, Session.alexaSessionDisplayType);
        }
    }
}

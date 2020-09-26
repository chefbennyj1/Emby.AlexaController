using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        public goBack(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            EmbyServerEntryPoint.Instance.Log.Info("Go Back requested.");

            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);

            EmbyServerEntryPoint.Instance.Log.Info("Go Back request Session: " + session.SessionId);
            var previousPage = session.paging.pages[session.paging.currentPage - 1];
            var currentPage = session.paging.pages[session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(session, currentPage, true);

            //if the user has requested an Emby client/room display during the session - go back on both if possible
            if (session.room != null)
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => EmbyServerEntryPoint.Instance.BrowseItemAsync(session,
                            EmbyServerEntryPoint.Instance.GetItemById(previousPage.baseItems[0].InternalId)))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch
                {
                }
            }

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(previousPage, session)
                }

            }, session.alexaSessionDisplayType);
        }
    }
}
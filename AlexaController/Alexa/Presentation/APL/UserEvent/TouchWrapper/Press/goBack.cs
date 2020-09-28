﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    // ReSharper disable once InconsistentNaming
    public class goBack : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        
        public goBack(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);

            var previousPage = session.paging.pages[session.paging.currentPage - 1];
            var currentPage  = session.paging.pages[session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(session, currentPage, true);

            //if the user is controlling a client  session - go back on the client too.
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

            var renderDocumentDirective = RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(previousPage, session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await renderDocumentDirective
                }

            }, session.alexaSessionDisplayType);
        }
    }
}
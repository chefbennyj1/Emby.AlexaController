﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    public class PreviousIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        
        public PreviousIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }
        public async Task<string> Response()
        {
            var sessionPaging = Session.paging;
            var previousPage  = sessionPaging.pages[sessionPaging.currentPage - 1];
            var currentPage   = sessionPaging.pages[sessionPaging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(Session, currentPage, true);

            //if the user has requested an Emby client/room display during the session - go back on both if possible
            // ReSharper disable once InvertIf
            if (Session.room != null)
                try
                {
#pragma warning disable 4014
                    ServerController.Instance.BrowseItemAsync(Session,ServerQuery.Instance.GetItemById(Session.NowViewingBaseItem.Parent.InternalId)).ConfigureAwait(false);
#pragma warning restore 4014
                } catch { }

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(previousPage, Session)
                }

            }, Session);
        }
    }
}

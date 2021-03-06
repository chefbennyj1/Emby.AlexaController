﻿using AlexaController.Alexa;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.UserEvent.TouchWrapper.Press
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
            var currentPage = session.paging.pages[session.paging.currentPage];

            AlexaSessionManager.Instance.UpdateSession(session, currentPage, true);

            //var properties = previousPage?.properties as Properties<MediaItem>;

            //if the user is controlling a client  session - go back on the client too.
            if (session.hasRoom)
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => ServerController.Instance.BrowseItemAsync(session, ServerDataQuery.Instance.GetItemById(previousPage?.item.id)))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            var renderDocumentDirective = RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync<MediaItem>(previousPage, session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await renderDocumentDirective
                }

            }, session);
        }
    }
}
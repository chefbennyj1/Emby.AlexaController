﻿using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource.DataSourceProperties;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.AMAZON
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class PreviousIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public PreviousIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var sessionPaging = Session.paging;
            var previousPage = sessionPaging.pages[sessionPaging.currentPage - 1];
            var currentPage = sessionPaging.pages[sessionPaging.currentPage];
            AlexaSessionManager.Instance.UpdateSession(Session, currentPage, true);

            var properties = previousPage as Properties<MediaItem>;

            //if the user is controlling a client  session - go back on the client too.
            // ReSharper disable once InvertIf
            if (Session.hasRoom)
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => ServerController.Instance.BrowseItemAsync(Session, ServerDataQuery.Instance.GetItemById(properties?.item.id)))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch (Exception exception)
                {
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(previousPage, Session)
                }

            }, Session);
        }
    }
}

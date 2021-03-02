﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventShowBaseItemDetailsTemplate : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        
        public UserEventShowBaseItemDetailsTemplate(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }
        public async Task<string> Response()
        {
            ServerController.Instance.Log.Info("UserEventShowBaseItemDetailsTemplate");
            var request        = AlexaRequest.request;
            var source         = request.source;
            var baseItem       = ServerQuery.Instance.GetItemById(source.id);
            var session        = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var room           = session.room;
            
            //var documentTemplateInfo = new RenderDocumentQuery()
            //{
            //    baseItems = new List<BaseItem>() {baseItem},
            //    renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            //};

            var dataSource = await DataSourceManager.Instance.GetBaseItemDetailsDataSourceAsync(baseItem, session);

            var renderAudioTemplateInfo = new AudioDirectiveQuery()
            {
                speechContent = SpeechContent.BROWSE_ITEM,
                session = session,
                items =  new List<BaseItem>() { baseItem },
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            // Update session data
            session.NowViewingBaseItem = baseItem;
            session.room               = room;
            AlexaSessionManager.Instance.UpdateSession(session, dataSource);
            
            //if the user has requested an Emby client/room display during the session - display both if possible
            if (!(room is null))
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => ServerController.Instance.BrowseItemAsync(session, baseItem))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch
                {
                }
            }

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, session);
            var renderAudioDirective    = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, session);
        }
    }
}

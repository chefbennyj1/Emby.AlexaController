﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
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
            var request        = AlexaRequest.request;
            var source         = request.source;
            var baseItem       = EmbyServerEntryPoint.Instance.GetItemById(source.id);
            var session        = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var room           = session.room;
            
            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() {baseItem},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            // Update session data
            session.NowViewingBaseItem = baseItem;
            session.room               = room;
            AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);
            
            //if the user has requested an Emby client/room display during the session - display both if possible
            if (!(room is null))
            {
                try
                {
#pragma warning disable 4014
                    Task.Run(() => EmbyServerEntryPoint.Instance.BrowseItemAsync(session, baseItem))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                }
                catch
                {
                }
            }

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.BROWSE_ITEM, session, new List<BaseItem>() { baseItem }),
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    /// <summary>
    /// request arguments[0] will be "UserEventPlaybackStart"
    /// request arguments[1] will be the room name
    /// </summary>
    
    public class UserEventPlaybackStart : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        

        public UserEventPlaybackStart(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }

        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var source = request.source;
            var session = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = EmbyServerEntryPoint.Instance.GetItemById(source.id);

            session.room = session.room ?? RoomManager.Instance.GetRoomByName(request.arguments[1]);

            RenderDocumentTemplate documentTemplateInfo = null;

            if (session.room is null)
            {
                documentTemplateInfo = new RenderDocumentTemplate()
                {
                    renderDocumentType = RenderDocumentType.ROOM_SELECTION_TEMPLATE,
                    baseItems = new List<BaseItem>() {baseItem}
                };

                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session)
                    }

                }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => EmbyServerEntryPoint.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014

            documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() {baseItem},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            var renderDocumentDirective =
                await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                person = session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, session, new List<BaseItem>() { baseItem }),
                   
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

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

            session.room = session.room ?? RoomContextManager.Instance.GetRoomByName(request.arguments[1]);
            
            
            if (session.room is null)
            {
                var documentTemplateInfo = new RenderDocumentTemplate()
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
                        await RenderDocumentBuilder.Instance.GetRenderDocumentAsync(documentTemplateInfo, session)
                    }

                }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => EmbyServerEntryPoint.Instance.PlayMediaItemAsync(session, baseItem));
#pragma warning restore 4014

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                person = session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase         = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, session, new List<BaseItem>() {baseItem}),
                    speechType = SpeechType.COMPLIANCE
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentAsync(new RenderDocumentTemplate()
                    {
                        baseItems          = new List<BaseItem>() {baseItem},
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, session)
                }
            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);   
            
        }
    }
}

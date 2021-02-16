using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.APLA.Filters;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;
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
            var request  = AlexaRequest.request;
            var source   = request.source;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var baseItem = ServerQuery.Instance.GetItemById(source.id);

            session.room = session.room ?? RoomManager.Instance.GetRoomByName(request.arguments[1]);

            RenderDocumentTemplate documentTemplateInfo = null;
            RenderAudioTemplate audioTemplateInfo = null;
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

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014

            documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() {baseItem},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            audioTemplateInfo = new RenderAudioTemplate()
            {
                speechContent = SpeechContent.PLAY_MEDIA_ITEM,
                session = session, 
                items = new List<BaseItem>() { baseItem },
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);
            var renderAudioDirective = await RenderAudioBuilder.Instance.GetAudioDirectiveAsync(audioTemplateInfo);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                //    {
                //        type = SpeechResponseType.PLAY_MEDIA_ITEM, 
                //        session = session, 
                //        items = new List<BaseItem>() { baseItem }
                //    }),
                   
                //},
                SpeakUserName = true,
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

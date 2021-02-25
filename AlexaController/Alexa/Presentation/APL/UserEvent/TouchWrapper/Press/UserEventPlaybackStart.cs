using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;
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

            InternalRenderDocumentQuery documentTemplateInfo = null;
            InternalRenderAudioQuery audioTemplateInfo = null;
            if (session.room is null)
            {
                documentTemplateInfo = new InternalRenderDocumentQuery()
                {
                    renderDocumentType = RenderDocumentType.ROOM_SELECTION_TEMPLATE,
                    baseItems = new List<BaseItem>() {baseItem}
                };

                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

                return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session)
                    }

                }, session);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session, null);

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(session, baseItem)).ConfigureAwait(false);
#pragma warning restore 4014

            documentTemplateInfo = new InternalRenderDocumentQuery()
            {
                baseItems = new List<BaseItem>() {baseItem},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            audioTemplateInfo = new InternalRenderAudioQuery()
            {
                speechContent = SpeechContent.PLAY_MEDIA_ITEM,
                session = session, 
                items = new List<BaseItem>() { baseItem },
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            var renderDocumentDirective = await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);
            var renderAudioDirective = await RenderAudioManager.Instance.GetAudioDirectiveAsync(audioTemplateInfo);

            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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

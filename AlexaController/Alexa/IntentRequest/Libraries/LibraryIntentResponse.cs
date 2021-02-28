using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Api;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;



namespace AlexaController.Alexa.IntentRequest.Libraries
{
    public class LibraryIntentResponse 
    {
        private string LibraryName { get; }

        public LibraryIntentResponse(string libraryName)
        {
            LibraryName = libraryName;
        }

        public async Task<string> Response(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            try { session.room = RoomManager.Instance.ValidateRoom(alexaRequest, session); } catch { }
            session.room = session.room;

            var context = alexaRequest.context;
            // we need the room object to proceed because we will only show libraries on emby devices

            if (session.room is null || (session.room is null && context.Viewport is null))
            {
                session.PersistedRequestContextData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session, null);
                return await RoomManager.Instance.RequestRoom(alexaRequest, session);
            }

            var request = alexaRequest.request;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = session
            //});

#pragma warning disable 4014
            Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse("One moment please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var result = ServerQuery.Instance.GetItemById(ServerQuery.Instance.GetLibraryId(LibraryName));

            try
            {
#pragma warning disable 4014
                Task.Run(() => ServerController.Instance.BrowseItemAsync(session, result)).ConfigureAwait(false);
#pragma warning restore 4014
            }
            catch (BrowseCommandException)
            {
                throw new BrowseCommandException($"Couldn't browse to {result.Name}");
            }

            session.NowViewingBaseItem = result;
            //reset rePrompt data because we have fulfilled the request
            session.PersistedRequestContextData = null;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            var documentTemplateInfo = new RenderDocumentQuery()
            {
                baseItems = new List<BaseItem>() {result},
                renderDocumentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE
            };

            var audioTemplateInfo = new AudioDirectiveQuery()
            {
                speechContent = SpeechContent.BROWSE_LIBRARY,
                session = session,
                items = new List<BaseItem>() { result },
                audio = new Audio()
                {
                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                    
                }
            };

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);
            var renderAudioDirective = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(audioTemplateInfo);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                //    {
                //        type = SpeechResponseType.BROWSE_LIBRARY, 
                //        session = session, 
                //        items = new List<BaseItem>() { result }
                //    })
                //},
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    renderDocumentDirective,
                    renderAudioDirective
                }

            }, session);
        }
    }
}


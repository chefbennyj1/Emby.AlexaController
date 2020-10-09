using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;
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
                session.PersistedRequestData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session, null);
                return await RoomManager.Instance.RequestRoom(alexaRequest, session);
            }

            var request = alexaRequest.request;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = session
            });

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var result = EmbyServerEntryPoint.Instance.GetItemById(EmbyServerEntryPoint.Instance.GetLibraryId(LibraryName));

            try
            {
#pragma warning disable 4014
                Task.Run(() => EmbyServerEntryPoint.Instance.BrowseItemAsync(session, result)).ConfigureAwait(false);
#pragma warning restore 4014
            }
            catch (BrowseCommandException)
            {
                throw new BrowseCommandException($"Couldn't browse to {result.Name}");
            }

            session.NowViewingBaseItem = result;
            //reset rePrompt data because we have fulfilled the request
            session.PersistedRequestData = null;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() {result},
                renderDocumentType = RenderDocumentType.BROWSE_LIBRARY_TEMPLATE
            };

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    {
                        type = SpeechResponseType.BROWSE_LIBRARY, 
                        session = session, 
                        items = new List<BaseItem>() { result }
                    })
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, session);
        }
    }
}


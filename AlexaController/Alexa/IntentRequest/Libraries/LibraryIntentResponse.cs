using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.Exceptions;
using AlexaController.PresentationManagers;
using AlexaController.Session;


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
            
            var context = alexaRequest.context;
            // we need the room object to proceed because we will only show libraries on emby devices

            if (session.room is null || (session.room is null && context.Viewport is null))
            {
                session.PersistedRequestContextData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session, null);
                return await RoomManager.Instance.RequestRoom(alexaRequest, session);
            }

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
            

            var aplDataSource  = await AplObjectDataSourceManager.Instance.GetGenericViewDataSource($"Showing the {result.Name} library", "/MoviesLibrary");
            var aplaDataSource = await AplAudioDataSourceManager.Instance.ItemBrowse(result, session);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, session);
            var renderAudioDirective    = await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
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


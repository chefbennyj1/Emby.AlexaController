using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Exceptions;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers;
using AlexaController.AlexaPresentationManagers;

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
            session.room = RoomManager.Instance.ValidateRoom(alexaRequest, session);
            session.hasRoom = !(session.room is null);
            if (!session.hasRoom && !session.supportsApl)
            {
                session.PersistedRequestContextData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session, null);
                return await RoomManager.Instance.RequestRoom(alexaRequest, session);
            }

            var libraryId = ServerQuery.Instance.GetLibraryId(LibraryName);
            var result = ServerQuery.Instance.GetItemById(libraryId);

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

            var aplDataSource           = await APL_DataSourceManager.Instance.GetGenericViewDataSource($"Showing the {result.Name} library", "/MoviesLibrary");
            var aplaDataSource          = await APLA_DataSourceManager.Instance.ItemBrowse(result, session);
            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<string>(aplDataSource, session);
            var renderAudioDirective    = await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource);

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


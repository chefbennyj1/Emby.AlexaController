using AlexaController.Alexa.ResponseModel;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Exceptions;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Libraries
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
            session.room = await RoomContextManager.Instance.ValidateRoom(alexaRequest, session);
            session.hasRoom = session.room != null;
            if (!session.hasRoom || !session.hasRoom && !session.supportsApl) //Still go if we have a room, but we don't support APL
            {
                session.PersistedRequestData = alexaRequest;
                AlexaSessionManager.Instance.UpdateSession(session, null);
                return await RoomContextManager.Instance.RequestRoom(alexaRequest, session);
            }

            var libraryId = ServerDataQuery.Instance.GetLibraryId(LibraryName);
            var result = ServerDataQuery.Instance.GetItemById(libraryId);

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
            session.PersistedRequestData = null;
            AlexaSessionManager.Instance.UpdateSession(session, null);

            var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync($"Showing the {result.Name} library", "/MoviesLibrary");
            var aplaDataSource = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.ItemBrowse,
                item = result,
                session = session
            });
            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(aplaDataSource);

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


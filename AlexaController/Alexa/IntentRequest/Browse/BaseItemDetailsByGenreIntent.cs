using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BaseItemDetailsByGenreIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public BaseItemDetailsByGenreIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }

        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }

        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
                Session.hasRoom = !(Session.room is null);
            }
            catch { }
            

            if (!Session.hasRoom && !Session.supportsApl) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = slots.MovieAlternatives.value is null ? "Series" : "Movie";
            var slotGenres     = slots.Genre;
            var genres         = GetGenreList(slotGenres);
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            var result = ServerQuery.Instance.GetBaseItemsByGenre(new [] { type }, genres.ToArray());

            IDataSource dataSource = null;

            if (result.TotalRecordCount <= 0)
            {
                dataSource = await AplObjectDataSourceManager.Instance.GetGenericViewDataSource("I was unable to find that. Does that genre exist?", "/Question");
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to that. Does that genre exist?",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    SpeakUserName    = true,
                    directives       = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(dataSource, Session)
                    }
                }, Session);
            }

            if (Session.hasRoom)
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, result.Items.FirstOrDefault());
                }
                catch (Exception exception)
                {
                    await Task.Run(() =>
                            AlexaResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }


            var phrase = "";
            
            for (var i = 0; i <= genres.Count -1; i++)
            {
                if (genres.Count - 1 > 0)
                {
                    if (i == genres.Count - 1)
                    {
                        phrase += $"and {genres[i]}";
                        break;
                    }
                    phrase += $"{genres[i]}, ";
                }
                else
                {
                    phrase += $"{genres[i]}";
                }
            }
            
            dataSource = await AplObjectDataSourceManager.Instance.GetSequenceItemsDataSourceAsync(result.Items.ToList());

            //Update Session
            Session.NowViewingBaseItem = result.Items[0];
            AlexaSessionManager.Instance.UpdateSession(Session, dataSource);

            var renderDocumentDirective = await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(dataSource, Session);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{type} Items with {phrase} genres.",
                    sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, Session);
        }

        private static List<string> GetGenreList(slotData slotGenres)
        {
            switch (slotGenres.slotValue.type)
            {
                case "Simple" : return new List<string>() { slotGenres.value };
                case "List"   : return slotGenres.slotValue.values.Select(v => v.value).ToList();
                default       : return null;
            }
        }
    }
}

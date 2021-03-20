using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.AlexaDataSourceManagers.DataSourceProperties;
using AlexaController.AlexaPresentationManagers;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class BaseItemDetailsByGenreIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public BaseItemDetailsByGenreIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public async Task<string> Response()
        {
            //await AlexaResponseClient.Instance.PostProgressiveResponse("OK.",
            //    AlexaRequest.context.System.apiAccessToken, AlexaRequest.request.requestId);

            Session.room = await RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            
            Session.hasRoom = !(Session.room is null);
            if (!Session.hasRoom && !Session.supportsApl)
            {
                Session.PersistedRequestContextData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session, null);
                return await RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            }

            var request    = AlexaRequest.request;
            var intent     = request.intent;
            var slots      = intent.slots;
            var type       = slots.MovieAlternatives.value is null ? "Series" : "Movie";
            var slotGenres = slots.Genre;
            
            var genres = GetGenreList(slotGenres);

            var result = ServerQuery.Instance.GetBaseItemsByGenre(new[] { type }, genres.ToArray());

            //IDataSource dataSource;

            if (result.TotalRecordCount <= 0)
            {
                var genericLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetGenericViewPropertiesAsync("I was unable to find that. Does that genre exist?", "/Question");
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to that. Does that genre exist?",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(genericLayoutProperties, Session)
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
                    ServerController.Instance.Log.Error(exception.Message);
                }
            }

            var phrase = "";

            for (var i = 0; i <= genres.Count - 1; i++)
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

            var sequenceLayoutProperties = await DataSourceLayoutPropertiesManager.Instance.GetSequenceViewPropertiesAsync(result.Items.ToList());

            //Update Session
            Session.NowViewingBaseItem = result.Items[0];
            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync(sequenceLayoutProperties, Session);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"{type} Items with {phrase} genres.",
                    sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
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
                case "Simple": return new List<string>() { slotGenres.value };
                case "List": return slotGenres.slotValue.values.Select(v => v.value).ToList();
                default: return null;
            }
        }
    }
}

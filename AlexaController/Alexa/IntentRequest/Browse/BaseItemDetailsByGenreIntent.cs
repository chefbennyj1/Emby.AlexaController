using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
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
            }
            catch { }
            

            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = slots.MovieAlternatives.value is null ? "Series" : "Movie";
            var slotGenres     = slots.Genre;
            var genres         = GetGenreList(slotGenres);
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            var result         = ServerQuery.Instance.GetBaseItemsByGenre(new [] { type }, genres.ToArray());

            if (result.TotalRecordCount <= 0)
            {
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find items. Does that genre exist?",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    SpeakUserName    = true,
                    directives       = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentQuery()
                        {
                            HeadlinePrimaryText = "I was unable to find items. Does that genre exist?",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session)
                    }
                }, Session);
            }

            if (!(Session.room is null))
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

            var documentTemplateInfo = new RenderDocumentQuery()
            {
                baseItems =  result.Items.ToList() ,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle = $"{type} Genres: {phrase}",
                //HeaderAttributionImage = actor.HasImage(ImageType.Primary) ? $"/Items/{actor?.Id}/Images/primary?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };
            
            //Update Session
            Session.NowViewingBaseItem = result.Items[0];
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
            
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BaseItemDetailsByGenreIntent : IIntentResponse
    {
        public BaseItemDetailsByGenreIntent(IAlexaRequest alexaRequest, IAlexaSession session)
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

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);


            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = slots.MovieAlternatives.value is null ? "Series" : "Movie";
            var slotGenres     = slots.Genre;

            var genres = new List<string>();

            switch (slotGenres.slotValue.type) {
                case "Simple":

                    ServerQuery.Instance.Log.Info($"Genre Intent Request: { type } { slotGenres.value} ");

                    genres.Add(slotGenres.slotValue.value);
                    break;
                case "List":
                {
                    foreach (var name in slotGenres.slotValue.values)
                    {
                        genres.Add(name.value);
                    }

                    break;
                }
            }

            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            //var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            //{
            //    type    = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = Session
            //});
            
            //await Task.Run(() => ResponseClient.Instance.PostProgressiveResponse($"{progressiveSpeech}, looking for {type} items with {(slotGenres.slotValue.type == "List" ? " those genres." : " that genre.")}", apiAccessToken, requestId)).ConfigureAwait(false);
            
            var result = ServerQuery.Instance.GetBaseItemsByGenre(new [] {type}, genres.ToArray());

            if (result is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find items.",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "I was unable to find items.",
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
                            ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
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
                        phrase += $"and {genres[i]}.";
                        break;
                    }
                    phrase += $"{genres[i]}, ";
                }
                else
                {
                    phrase += $"{genres[i]}";
                }
            }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems =  result.Items.ToList() ,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle = $"Genres: {phrase}",
                //HeaderAttributionImage = actor.HasImage(ImageType.Primary) ? $"/Items/{actor?.Id}/Images/primary?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };
            
            //Update Session
            Session.NowViewingBaseItem = result.Items[0];
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
            
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Items with {phrase} genres.",
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
    }
}

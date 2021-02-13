using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BaseItemsByActorIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public BaseItemsByActorIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
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

            var searchNames    = GetActorList(slots);

            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = Session
            });

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse($"{progressiveSpeech}, looking for library items by {(slots.ActorName.slotValue.type == "List" ? " those actors." : " that actor.")}", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var result = ServerQuery.Instance.GetItemsByActor(Session.User, searchNames);

            if (result is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find that actor.",
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "I was unable to find that actor.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session)
                    }
                }, Session);
            }

            if (!(Session.room is null))
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, result.Keys.FirstOrDefault().FirstOrDefault());
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

            var actors = result.Keys.FirstOrDefault();
            
            var actorCollection = result.Values.FirstOrDefault();

            var phrase = "";
            
            for (var i = 0; i <= actors?.Count -1; i++)
            {
                if (actors.Count - 1 > 0)
                {
                    if (i == actors.Count - 1)
                    {
                        phrase += $"and {actors[i].Name}.";
                        break;
                    }
                    phrase += $"{actors[i].Name}, ";
                }
                else
                {
                    phrase += $"{actors[i].Name}";
                }
            }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems =  actorCollection ,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle = $"Starring {phrase}",
                //HeaderAttributionImage = actor.HasImage(ImageType.Primary) ? $"/Items/{actor?.Id}/Images/primary?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            //TODO: Fix session Update (it is only looking at one actor, might not matter)
            //Update Session
            Session.NowViewingBaseItem = actors[0];
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

           

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Items starring {phrase}",
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

        private static List<string> GetActorList(Slots slots)
        {
            switch (slots.ActorName.slotValue.type)
            {
                case "Simple" : return new List<string>() { slots.ActorName.value };
                case "List"   : return slots.ActorName.slotValue.values.Select(a => a.value).ToList();
                default       : return null;
            }
        }
    }
}

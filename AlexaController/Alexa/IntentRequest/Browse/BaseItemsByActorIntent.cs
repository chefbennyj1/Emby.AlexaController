using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class BaseItemsByActorIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public BaseItemsByActorIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public string Response()
        {
            try
            {
                Session.room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);


            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var searchName = slots.ActorName.value;

            var context = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var progressiveSpeech = "";
            progressiveSpeech += $"{SpeechSemantics.SpeechResponse(SpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SpeechSemantics.SpeechResponse(SpeechType.REPOSE)}";
            ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            var result = EmbyServerEntryPoint.Instance.GetItemsByActor(Session.User, searchName);

            if (result is null)
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find that actor.",
                        speechType = SpeechType.APOLOGETIC,
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "I was unable to find that actor.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session)
                    }
                }, Session.alexaSessionDisplayType);
            }

            if (!(Session.room is null))
                try
                {
                    EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, result.Keys.FirstOrDefault());
                }
                catch (Exception exception)
                {
                    ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    Session.room = null;
                }

            Task.Delay(1200);

            var actor = result.Keys.FirstOrDefault();
            var actorCollection = result.Values.FirstOrDefault();

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems =  actorCollection ,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                HeaderTitle = $"Staring {actor?.Name}",
                HeaderAttributionImage = actor.HasImage(ImageType.Primary) ? $"/Items/{actor?.Id}/Images/primary?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            //Update Session
            Session.NowViewingBaseItem = actor;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = searchName,
                    sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                }

            }, Session.alexaSessionDisplayType);
        }
    }
}

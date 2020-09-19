using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    public class BrowseBaseItemsByActorIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public BrowseBaseItemsByActorIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);
            
            var request = AlexaRequest.request;
            var intent = request.intent;
            var slots = intent.slots;
            var searchName = slots.ActorName.value;

            var context = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            Alexa.ResponseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            var result = EmbyControllerUtility.Instance.GetItemsByActor(Session.User, searchName);

            if (result is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "I was unable to find that actor.",
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
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

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, Session.User, result.Keys.FirstOrDefault());
                }
                catch (Exception exception)
                {
                    Alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems =  result.Values.FirstOrDefault() ,
                renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE
            };

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
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

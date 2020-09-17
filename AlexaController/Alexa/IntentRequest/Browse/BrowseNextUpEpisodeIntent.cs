using System;
using System.Collections.Generic;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BrowseNextUpEpisodeIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public BrowseNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone)
                return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);

            var request = AlexaRequest.request;
            var context = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId = request.requestId;

            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";

            Alexa.ResponseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);
            
            var nextUpEpisode = EmbyControllerUtility.Instance.GetNextUpEpisode(request.intent, Session.User);
            
            if (nextUpEpisode is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE, Session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                        sound          = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    shouldEndSession = true,
                    directives       = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            HeadlinePrimaryText = "There doesn't seem to be a new episode available.",
                            renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                        }, Session)
                    }
                }, Session.alexaSessionDisplayType);
            }
            
            //Parental Control check for baseItem
            if (!nextUpEpisode.IsParentalAllowed(Session.User))
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>(){nextUpEpisode}),
                        sound     = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"

                    },
                    shouldEndSession = true
                });
            }

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, Session.User, nextUpEpisode);
                }
                catch (Exception exception)
                {
                    Alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems          = new List<BaseItem>() {nextUpEpisode},
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            Session.NowViewingBaseItem = nextUpEpisode;
            Session.room = room;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_NEXT_UP_EPISODE, Session , new List<BaseItem>() {nextUpEpisode}),
                    sound          = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                },
                shouldEndSession = null,
                directives       = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                }

            }, Session.alexaSessionDisplayType);
           
        }
    }
}

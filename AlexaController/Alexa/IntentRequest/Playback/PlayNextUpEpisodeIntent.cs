using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

// ReSharper disable once PossibleNullReferenceException
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public PlayNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            //we need a room object
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            if (room is null) return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);

            var request = AlexaRequest.request;
            var nextUpEpisode = EmbyControllerUtility.Instance.GetNextUpEpisode(request.intent, Session?.User);

            if (nextUpEpisode is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                });
            }

            EmbyControllerUtility.Instance.PlayMediaItemAsync(Session, nextUpEpisode, Session?.User);

            Session.NowViewingBaseItem = nextUpEpisode;
            Session.room = room;
            AlexaSessionManager.Instance.UpdateSession(Session);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PLAY_NEXT_UP_EPISODE, Session, new List<BaseItem>() { nextUpEpisode }),
                    semanticSpeechType = SemanticSpeechType.COMPLIANCE,
                },
                shouldEndSession = true,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                    {
                        baseItems          = new List<BaseItem>() { nextUpEpisode },
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, Session)
                }
            }, Session.alexaSessionDisplayType);
        }
    }
}

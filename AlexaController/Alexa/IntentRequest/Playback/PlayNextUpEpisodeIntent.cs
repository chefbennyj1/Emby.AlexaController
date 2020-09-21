using System.Collections.Generic;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
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
        

        public PlayNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            ;
            Session = session;
            ;
        }
        public string Response()
        {
            //we need a room object
            Room room = null;
            try { room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
            if (room is null) return RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            Session.room = room;

            var request = AlexaRequest.request;
            var nextUpEpisode = EmbyServerEntryPoint.Instance.GetNextUpEpisode(request.intent, Session?.User);

            if (nextUpEpisode is null)
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                        speechType = SpeechType.APOLOGETIC,
                    },
                });
            }

            EmbyServerEntryPoint.Instance.PlayMediaItemAsync(Session, nextUpEpisode);

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_NEXT_UP_EPISODE, Session, new List<BaseItem>() { nextUpEpisode }),
                    speechType = SpeechType.COMPLIANCE,
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

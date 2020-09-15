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

// ReSharper disable once PossibleNullReferenceException
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IIntentResponseModel
    {
        public string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            //we need a room object
            var roomManager = new RoomContextManager();
            Room room = null;
            try { room = roomManager.ValidateRoom(alexaRequest, session); } catch { }
            if (room is null) return roomManager.RequestRoom(alexaRequest, session, responseClient);

            var request = alexaRequest.request;
            var nextUpEpisode = EmbyControllerUtility.Instance.GetNextUpEpisode(request.intent, session?.User);

            if (nextUpEpisode is null)
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                });
            }

            EmbyControllerUtility.Instance.PlayMediaItemAsync(session, nextUpEpisode, session?.User);

            session.NowViewingBaseItem = nextUpEpisode;
            session.room = room;
            AlexaSessionManager.Instance.UpdateSession(session);

            return responseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PLAY_NEXT_UP_EPISODE, session, new List<BaseItem>() { nextUpEpisode }),
                    semanticSpeechType = SemanticSpeechType.COMPLIANCE,
                },
                shouldEndSession = true,
                directives = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                    {
                        baseItems          = new List<BaseItem>() { nextUpEpisode },
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, session)
                }
            }, session.alexaSessionDisplayType);
        }
    }
}

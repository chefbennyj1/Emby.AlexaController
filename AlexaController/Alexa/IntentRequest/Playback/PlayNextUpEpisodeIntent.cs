using System.Collections.Generic;
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

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var request = alexaRequest.request;

            //check the room
            //var room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);//(request.intent.slots.Room.value ?? session.room) ?? string.Empty;

            Room room = null;
            try
            {
                room = AlexaSessionManager.Instance.ValidateRoom(alexaRequest, session);
            }
            catch
            {
            }

            if (room is null)
                return responseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.ROOM_CONTEXT, session, null),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    }
                });

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

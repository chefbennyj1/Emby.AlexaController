using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
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
            Session      = session;
        }
        public async Task<string> Response()
        {
            //we need a room object

            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            } catch { }
            if (Session.room is null) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request = AlexaRequest.request;
            var nextUpEpisode = EmbyServerEntryPoint.Instance.GetNextUpEpisode(request.intent, Session?.User);

            if (nextUpEpisode is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session)
                    },
                });
            }

#pragma warning disable 4014
            Task.Run(() => EmbyServerEntryPoint.Instance.PlayMediaItemAsync(Session, nextUpEpisode));
#pragma warning restore 4014

            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_NEXT_UP_EPISODE, Session, new List<BaseItem>() { nextUpEpisode }),
                   
                },
                shouldEndSession = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                    {
                        baseItems          = new List<BaseItem>() { nextUpEpisode },
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, Session)
                }
            }, Session.alexaSessionDisplayType);
        }
    }
}

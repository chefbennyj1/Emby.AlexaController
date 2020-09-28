using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

// ReSharper disable TooManyChainedReferences
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayItemIntent : IIntentResponse
    {
        //If no room is requested in the PlayItemIntent intent, we follow up immediately to get a room value from 'RoomName' intent. 

        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        
        public PlayItemIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }

        public async Task<string> Response()
        {
            try { Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
            if (Session.room is null) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            var intent         = request.intent;
            var slots          = intent.slots;

            var progressiveSpeech = SpeechStrings.GetPhrase(SpeechResponseType.PROGRESSIVE_RESPONSE, Session);

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            BaseItem result = null;
            if (Session.NowViewingBaseItem is null)
            {
                var type = slots.Movie.value is null ? "Series" : "Movie";
                result = EmbyServerEntryPoint.Instance.QuerySpeechResultItem(
                    type == "Movie" ? slots.Movie.value : slots.Series.value, new[] { type }, Session.User);
            }
            else
            {
                result = Session.NowViewingBaseItem;
            }
            
            //Item doesn't exist in the library
            if (result is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                       
                    }
                });
            }
            
            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(Session.User))
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>",
                       
                    }
                });
            }

            try
            {
#pragma warning disable 4014
                Task.Run(() => EmbyServerEntryPoint.Instance.PlayMediaItemAsync(Session, result)).ConfigureAwait(false);
#pragma warning restore 4014
            }
            catch (Exception exception)
            {
#pragma warning disable 4014
                Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
                await Task.Delay(1200);
            }

            Session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                baseItems = new List<BaseItem>() {result}
            };

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, Session, new List<BaseItem>() { result })
                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    renderDocumentDirective
                }

            }, Session.alexaSessionDisplayType);

        }
    }
}

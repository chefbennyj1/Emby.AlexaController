using System;
using System.Collections.Generic;
using AlexaController.Alexa.Exceptions;
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
        public string Response()
        {
            //we need a room object
            //Room room = null;
            try { Session.room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session); } catch { }
            if (Session.room is null) return RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            
            EmbyServerEntryPoint.Instance.Log.Info("ALEXA REQUESTED ROOM " + Session.room.Name + " TO PLAY ITEMS");

            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            var intent         = request.intent;
            var slots          = intent.slots;

            ResponseClient.Instance.PostProgressiveResponse($"{SpeechSemantics.SpeechResponse(SpeechType.COMPLIANCE)} {SpeechSemantics.SpeechResponse(SpeechType.REPOSE)}", apiAccessToken, requestId);

            BaseItem result = null;
            if (Session.NowViewingBaseItem is null)
            {
                var type = slots.Movie.value is null ? "Series" : "Movie";
                result = EmbyServerEntryPoint.Instance.QuerySpeechResultItem(
                    type == "Movie" 
                        ? slots.Movie.value 
                        : slots.Series.value, new[] {type}, Session.User);
            }
            else
            {
                result = Session.NowViewingBaseItem;
            }
            //var result = Session.NowViewingBaseItem ??
            //             (!(request.intent.slots.Movie.value is null)
            //                 ? EmbyServerEntryPoint.Instance.QuerySpeechResultItem(request.intent.slots.Movie.value, new[] { "Movie" }, Session.User)
            //                 : !(request.intent.slots.Series.value is null)
            //                    ? EmbyServerEntryPoint.Instance.QuerySpeechResultItem(request.intent.slots.Series.value, new[] { "Series" }, Session.User) : null);

            //If result is null here, then the item doesn't exist in the library
            if (result is null)
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                        speechType = SpeechType.APOLOGETIC
                    }
                });
            }
            
            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(Session.User))
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>",
                        speechType = SpeechType.APOLOGETIC
                    }
                });
            }

            

            try
            {
                EmbyServerEntryPoint.Instance.PlayMediaItemAsync(Session, result);
            }
            catch (Exception exception)
            {
                EmbyServerEntryPoint.Instance.Log.Error("ALEXA ERROR!! : " + exception.Message);
                //TODO: Add progressive response with error, but show template on screen device is possible
                return new ErrorHandler().OnError(exception, AlexaRequest, Session);
            }

            Session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(Session);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, Session, new List<BaseItem>() { result })

                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                        {
                            renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                            baseItems          = new List<BaseItem>() { result },
                            
                        }, Session)
                    }

            }, Session.alexaSessionDisplayType);

        }
    }
}

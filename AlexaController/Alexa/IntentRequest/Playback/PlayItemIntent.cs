using System;
using System.Collections.Generic;
using AlexaController.Alexa.Errors;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;

// ReSharper disable TooManyChainedReferences
// ReSharper disable once ComplexConditionExpression

namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayItemIntent : IIntentResponse
    {
        /*         
            If no room is requested in the PlayItemIntent intent, we follow up immediately to get a room value from 'RoomName' intent. 
            We hold the requested baseItem intent name value in 'sessions.RepromptIntent' to search for later.

            If a room is requested then we search for a baseItem value to play.                
         
            If a user has been browsing the library on an AMAZON screen device the 'session.NowViewingItem' 
            will contain the baseItem to play, and no library search will be needed.

            If no value is stored in 'session.NowViewingItem', the device is most likely 'non-screen' device from AMAZON.
                     
            We'll search the library using the 'request.intent.slots.Movie.value' or 'request.intent.slots.TvSeries.value', respectively.

            Play back is sent to the Emby device.         
                                
         */

        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        public IAlexaEntryPoint Alexa     { get; }

        public PlayItemIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa        = alexa;
            Session      = session;
            Alexa        = alexa;
        }
        public string Response()
        {
            //we need a room object
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            if (room is null) return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);

            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            Alexa.ResponseClient.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);


            var result = Session.NowViewingBaseItem ??
                         (!(request.intent.slots.Movie.value is null)
                             ? EmbyControllerUtility.Instance.QuerySpeechResultItem(request.intent.slots.Movie.value, new[] { "Movie" }, Session.User)
                             : !(request.intent.slots.Series.value is null)
                                ? EmbyControllerUtility.Instance.QuerySpeechResultItem(request.intent.slots.Series.value, new[] { "Series" }, Session.User) : null);

            //If result is null here, then the item doesn't exist in the library
            if (result is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC
                    }
                });
            }

            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(Session.User))
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>",
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC
                    }
                });
            }

            try
            {
                EmbyControllerUtility.Instance.PlayMediaItemAsync(Session, result, Session.User);
            }
            catch (Exception exception)
            {
                //TODO: Add progressive response with error, but show template on screen device is possible
                return new ErrorHandler().OnError(exception, AlexaRequest, Session, Alexa.ResponseClient);
            }

            Session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(Session);

            return Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, Session, new List<BaseItem>() { result })

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

            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);

        }
    }
}

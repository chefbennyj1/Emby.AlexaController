using System;
using System.Collections.Generic;
using AlexaController.Alexa.Errors;
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

// ReSharper disable TooManyChainedReferences
// ReSharper disable once ComplexConditionExpression
// ReSharper disable TooManyArguments

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

        public string Response
        (IAlexaRequest alexaRequest, IAlexaSession session, AlexaEntryPoint alexa)//, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager, IRoomContextManager roomContextManager)
        {
            //we need a room object
            Room room = null;
            try { room = alexa.RoomContextManager.ValidateRoom(alexaRequest, session); } catch { }
            if (room is null) return alexa.RoomContextManager.RequestRoom(alexaRequest, session, alexa.ResponseClient);

            var request        = alexaRequest.request;
            var context        = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            alexa.ResponseClient.PostProgressiveResponse($"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} {SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}", apiAccessToken, requestId);


            var result = session.NowViewingBaseItem ??
                         (!(request.intent.slots.Movie.value is null)
                             ? EmbyControllerUtility.Instance.QuerySpeechResultItems(request.intent.slots.Movie.value, new[] { "Movie" }, session.User)
                             : !(request.intent.slots.Series.value is null)
                                ? EmbyControllerUtility.Instance.QuerySpeechResultItems(request.intent.slots.Series.value, new[] { "Series" }, session.User) : null);

            //If result is null here, then the item doesn't exist in the library
            if (result is null)
            {
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase    = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, session),
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC
                    }
                });
            }


            //Parental Control check for baseItem
            if (!result.IsParentalAllowed(session.User))
            {
                return alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>",
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC
                    }
                });
            }

            try
            {

                EmbyControllerUtility.Instance.PlayMediaItemAsync(session, result, session.User);
            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(exception, alexaRequest, session, alexa.ResponseClient);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session);

            return alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, session, new List<BaseItem>() { result })

                },
                shouldEndSession = null,
                directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                            baseItems          = new List<BaseItem>() { result },
                            
                        }, session)
                    }

            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);

        }
    }
}

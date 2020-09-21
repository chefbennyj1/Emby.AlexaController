using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Exceptions;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Configuration;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    
    [Intent]
    public class BaseItemDetailsByNameIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        

        public BaseItemDetailsByNameIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public string Response()
        {

            try
            {
                Session.room = RoomContextManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return RoomContextManager.Instance.RequestRoom(AlexaRequest, Session);
            

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = request.intent.slots.Movie.value is null ? "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            var compliance = SpeechSemantics.SpeechResponse(SpeechType.COMPLIANCE);
            var repose = SpeechSemantics.SpeechResponse(SpeechType.REPOSE);
            var progressiveSpeech = string.Join(" ", compliance, repose);
            ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            //Clean up search term
            searchName = StringNormalization.ValidateSpeechQueryString(searchName);

            if (string.IsNullOrEmpty(searchName)) return new NotUnderstood(AlexaRequest, Session).Response(); 

            var result = EmbyServerEntryPoint.Instance.QuerySpeechResultItem(searchName, new[] { type }, Session.User);

            if (result is null)
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                    }
                });
            }
            
            if (!result.IsParentalAllowed(Session.User))
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    }
                });

            if (!(Session.room is null))
                try
                {
                    EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, result);
                }
                catch (Exception exception)
                {
                    ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    Session.room = null;
                }

            Task.Delay(1200); //Yep...
            
            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() { result },
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                HeaderAttributionImage = result.HasImage(ImageType.Logo) ? $"/Items/{result.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            //Update Session
            Session.NowViewingBaseItem = result;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);
            
            try
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SpeechStrings.GetPhrase(SpeechResponseType.BROWSE_ITEM, Session, new List<BaseItem> { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                    },
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                    }

                }, Session.alexaSessionDisplayType);

            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(exception.InnerException, AlexaRequest, Session);
            }
        }
    }
}
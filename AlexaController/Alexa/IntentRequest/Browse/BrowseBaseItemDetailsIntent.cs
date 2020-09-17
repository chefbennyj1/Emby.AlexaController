using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Browse
{
    
    [Intent]
    public class BrowseBaseItemDetailsIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }
        public IAlexaEntryPoint Alexa     { get; }

        public BrowseBaseItemDetailsIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            Room room = null;
            try { room = Alexa.RoomContextManager.ValidateRoom(AlexaRequest, Session); } catch { }
            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (room is null && displayNone) return Alexa.RoomContextManager.RequestRoom(AlexaRequest, Session, Alexa.ResponseClient);
            
            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = request.intent.slots.Movie.value is null ? "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            var progressiveSpeech = "";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.COMPLIANCE)} ";
            progressiveSpeech += $"{SemanticSpeechUtility.GetSemanticSpeechResponse(SemanticSpeechType.REPOSE)}";
            Alexa.ResponseClient.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId);

            //Clean up search term
            searchName = StringNormalization.NormalizeText(searchName);

            if (string.IsNullOrEmpty(searchName)) return new NotUnderstood(AlexaRequest, Session, Alexa).Response(); 

            var result = EmbyControllerUtility.Instance.QuerySpeechResultItem(searchName, new[] { type }, Session.User);

            if (result is null)
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, Session),
                    }
                });
            }
            
            if (!result.IsParentalAllowed(Session.User))
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED, Session, new List<BaseItem>() { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    }
                });

            if (!(room is null))
                try
                {
                    EmbyControllerUtility.Instance.BrowseItemAsync(room.Name, Session.User, result);
                }
                catch (Exception exception)
                {
                    Alexa.ResponseClient.PostProgressiveResponse(exception.Message, apiAccessToken, requestId);
                    room = null;
                }

            Task.Delay(1500); //<-Yep...

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() { result },
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE
            };

            //Update Session
            Session.NowViewingBaseItem = result;
            Session.room = room;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);
            
            try
            {
                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = SemanticSpeechStrings.GetPhrase(SpeechResponseType.BROWSE_ITEM, Session, new List<BaseItem> { result }),
                        sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                    },
                    person = Session.person,
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                    }

                }, Session.alexaSessionDisplayType);

            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(exception.InnerException, AlexaRequest, Session, Alexa.ResponseClient);
            }
        }
    }
}
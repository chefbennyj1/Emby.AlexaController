using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;

namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
    public class NewItemsIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public NewItemsIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var request        = AlexaRequest.request;
            var slots          = request.intent.slots;
            var duration       = slots.Duration.value;
            var type           = slots.MovieAlternatives.value is null ? "New TV Shows" : "New Movies";
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            // Default will be 25 days ago unless given a time duration
            var d = duration is null ? DateTime.Now.AddDays(-25)
                : DateTimeDurationNormalization
                    .GetMinDateCreation(DateTimeDurationNormalization.DeserializeDurationFromIso8601(duration));

            ResponseClient.Instance.PostProgressiveResponse($"Looking for {type}", apiAccessToken, requestId);

            var query = type == "New TV Shows"
                ? EmbyServerEntryPoint.Instance.GetLatestTv(Session.User, d)
                : EmbyServerEntryPoint.Instance.GetLatestMovies(Session.User, d);

            var results = query.Where(item => item.IsParentalAllowed(Session.User)).ToList();

            if (!results.Any())
            {
                return ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"No { type } have been added.",
                        speechType = SpeechType.APOLOGETIC,
                    },
                    person = Session.person,
                    shouldEndSession = true,

                }, Session.alexaSessionDisplayType).Result;
            }
           
            switch (Session.alexaSessionDisplayType)
            {
                case AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE:
                {
                        var documentTemplateInfo = new RenderDocumentTemplate()
                        {
                            baseItems          = results,
                            renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                            HeaderTitle        = type
                        };

                        AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase             = SpeechStrings.GetPhrase(SpeechResponseType.NEW_ITEMS_APL, Session, results),
                                speechType = SpeechType.COMPLIANCE,
                            },
                            person           = Session.person,
                            shouldEndSession = null,
                            directives       = new List<IDirective>()
                            {
                                RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, Session)
                            }

                        }, Session.alexaSessionDisplayType);
                    }
                default: //Voice only
                    {
                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase             = SpeechStrings.GetPhrase(SpeechResponseType.NEW_ITEMS_DISPLAY_NONE, Session, results),
                                speechType = SpeechType.COMPLIANCE,
                            },
                            person           = Session.person,
                            shouldEndSession = true,

                        }, Session.alexaSessionDisplayType);
                    }
            }
        }
    }
}

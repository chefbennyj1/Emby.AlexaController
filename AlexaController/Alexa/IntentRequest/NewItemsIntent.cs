using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable twice TooManyChainedReferences
// ReSharper disable once TooManyArguments
// ReSharper disable once InconsistentNaming

namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
    public class NewItemsIntent : IIntentResponseModel
    {
        public  string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var request        = alexaRequest.request;
            var slots          = request.intent.slots;
            var duration       = slots.Duration.value;
            var type           = slots.MovieAlternatives.value is null ? "New TV Shows" : "New Movies";
            var context        = alexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
            
            // Default will be 25 days ago unless given a time period duration
            var d = duration is null ? DateTime.Now.AddDays(-25)
                : DateTimeDurationNormalization
                    .GetMinDateCreation(DateTimeDurationNormalization.DeserializeDurationFromIso8601(duration));

            //TODO: Respond with the time frame the user request: "Looking for new movies from the last thrity days"
            responseClient.PostProgressiveResponse($"Looking for {type}", apiAccessToken, requestId);

            var results = type == "New TV Shows" ? EmbyControllerUtility.Instance.GetLatestTv(session.User, d).ToList()
                : EmbyControllerUtility.Instance.GetLatestMovies(session.User, d)
                    .Where(movie => movie.IsParentalAllowed(session.User)).ToList();

            if (!results.Any())
            {
                return responseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"No { type } have been added during that time.",
                        semanticSpeechType = SemanticSpeechType.APOLOGETIC,
                    },
                    person = session.person,
                    shouldEndSession = true,

                }, session.alexaSessionDisplayType);
            }
           
            switch (session.alexaSessionDisplayType)
            {
                case AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE:
                {
                        var documentTemplateInfo = new RenderDocumentTemplateInfo()
                        {
                            baseItems          = results,
                            renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                            HeaderTitle        = type
                        };

                        AlexaSessionManager.Instance.UpdateSession(session, documentTemplateInfo);

                        return responseClient.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase             = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NEW_ITEMS_APL, session, results),
                                semanticSpeechType = SemanticSpeechType.COMPLIANCE,
                            },
                            person           = session.person,
                            shouldEndSession = null,
                            directives       = new List<Directive>()
                            {
                                RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(documentTemplateInfo, session)
                            }

                        }, session.alexaSessionDisplayType);
                    }
                default: //Voice only
                    {
                        return responseClient.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase             = SemanticSpeechStrings.GetPhrase(SpeechResponseType.NEW_ITEMS_DISPLAY_NONE, session, results),
                                semanticSpeechType = SemanticSpeechType.COMPLIANCE,
                            },
                            person           = session.person,
                            shouldEndSession = true,

                        }, session.alexaSessionDisplayType);
                    }
            }
        }
    }
}

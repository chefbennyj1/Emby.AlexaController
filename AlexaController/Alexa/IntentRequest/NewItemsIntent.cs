using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;

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
            var d = duration is null ? DateTime.Now.AddDays(-25) : DateTimeDurationSerializer.GetMinDateCreation(duration);

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse($"Looking for {type}", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var query = type == "New TV Shows"
                ? ServerQuery.Instance.GetLatestTv(Session.User, d)
                : ServerQuery.Instance.GetLatestMovies(Session.User, d);

            var results = query.Where(item => item.IsParentalAllowed(Session.User)).ToList();

            if (!results.Any())
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"No { type } have been added."
                    },
                    shouldEndSession = true,
                    SpeakUserName = true,
                }, Session);
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

                        var renderDocumentDirective =
                            await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                                {
                                    type = SpeechResponseType.NEW_ITEMS_APL, 
                                    session = Session, 
                                    items = results,
                                    args = new []{d.ToLongDateString()}
                                })
                            },
                            shouldEndSession = null,
                            SpeakUserName = true,
                            directives       = new List<IDirective>()
                            {
                                renderDocumentDirective
                            }

                        }, Session);
                    }
                default: //Voice only
                    {
                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                                {
                                    type = SpeechResponseType.NEW_ITEMS_DISPLAY_NONE, 
                                    session = Session, 
                                    items = results,
                                    args = new []{d.ToLongDateString()}
                                })
                            },
                            shouldEndSession = true,
                            SpeakUserName = true,

                        }, Session);
                    }
            }
        }
    }
}

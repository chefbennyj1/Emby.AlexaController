using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Viewport;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using AlexaController.Utils;

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
            Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse($"Looking for {type}", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var query = type == "New TV Shows"
                ? ServerQuery.Instance.GetLatestTv(Session.User, d)
                : ServerQuery.Instance.GetLatestMovies(Session.User, d);

            var results = query.Where(item => item.IsParentalAllowed(Session.User)).ToList();

            if (!results.Any())
            {
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"No { type } have been added."
                    },
                    shouldEndSession = true,
                    SpeakUserName = true,
                }, Session);
            }
           

            switch (Session.viewport)
            {
                case ViewportProfile.HUB_ROUND_SMALL:
                case ViewportProfile.HUB_LANDSCAPE_SMALL:
                case ViewportProfile.HUB_LANDSCAPE_MEDIUM:
                case ViewportProfile.HUB_LANDSCAPE_LARGE:
                {
                        var documentTemplateInfo = new RenderDocumentQuery()
                        {
                            baseItems          = results,
                            renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                            HeaderTitle        = type
                        };

                        var renderAudioTemplateInfo = new AudioDirectiveQuery()
                        {
                            speechContent = SpeechContent.NEW_ITEMS_APL,
                            session = Session, 
                            items = results,
                            args = new []{d.ToLongDateString()},
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        };

                        AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

                        var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
                        var renderAudioDirective    = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo);

                        return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                        {
                            //outputSpeech = new OutputSpeech()
                            //{
                            //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                            //    {
                            //        type = SpeechResponseType.NEW_ITEMS_APL, 
                            //        session = Session, 
                            //        items = results,
                            //        args = new []{d.ToLongDateString()}
                            //    })
                            //},
                            shouldEndSession = null,
                            SpeakUserName = true,
                            directives       = new List<IDirective>()
                            {
                                renderDocumentDirective,
                                renderAudioDirective
                            }

                        }, Session);
                    }
                default: //Voice only
                    {
                        var renderAudioTemplateInfo = new AudioDirectiveQuery()
                        {
                            speechContent = SpeechContent.NEW_ITEMS_DISPLAY_NONE,
                            session = Session, 
                            items = results,
                            args = new []{d.ToLongDateString()}
                        };

                        var renderAudioDirective    = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo);
                        return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                        {
                            //outputSpeech = new OutputSpeech()
                            //{
                            //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                            //    {
                            //        type = SpeechResponseType.NEW_ITEMS_DISPLAY_NONE, 
                            //        session = Session, 
                            //        items = results,
                            //        args = new []{d.ToLongDateString()}
                            //    })
                            //},
                            shouldEndSession = true,
                            SpeakUserName = true,
                            directives       = new List<IDirective>()
                            {
                                renderAudioDirective
                            }

                        }, Session);
                    }
            }
        }
    }
}

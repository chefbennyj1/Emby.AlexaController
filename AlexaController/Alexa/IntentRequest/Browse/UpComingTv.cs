using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class UpComingTv : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public UpComingTv(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var request        = AlexaRequest.request;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;
           
            var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = Session
            });

#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var slots          = request.intent.slots;
            var durationValue  = slots.Duration.value;
            var duration       = durationValue is null ? DateTime.Now.AddDays(7) : DateTimeDurationSerializer.GetMaxPremiereDate(durationValue);
            
            var results = await ServerQuery.Instance.GetUpComingTvAsync(duration);

            switch (Session.alexaSessionDisplayType)
            {
                case AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE:
                    {
                        var documentTemplateInfo = new RenderDocumentTemplate()
                        {
                            baseItems = results,
                            renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                            HeaderTitle = "Upcoming Episode"
                        };

                        AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

                        var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            outputSpeech = new OutputSpeech()
                            {
                                phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                                {
                                    type = SpeechResponseType.UP_COMING_EPISODES,
                                    session = Session,
                                    items = results,
                                    args = new []{duration.ToLongDateString()}
                                })
                            },
                            shouldEndSession = null,
                            directives = new List<IDirective>()
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
                                    type = SpeechResponseType.UP_COMING_EPISODES,
                                    session = Session,
                                    items = results,
                                    args = new []{duration.ToLongDateString()}
                                })
                            },
                            SpeakUserName = true,
                            shouldEndSession = true,

                        }, Session);
                    }
            }
        }
    }
}

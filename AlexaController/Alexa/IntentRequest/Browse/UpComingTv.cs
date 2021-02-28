using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using AlexaController.Utils;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class UpComingTv : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public UpComingTv(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
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
           
            //var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            //{
            //    type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
            //    session = Session
            //});

#pragma warning disable 4014
            Task.Run(() => AlexaResponseClient.Instance.PostProgressiveResponse("One moment please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014

            var slots          = request.intent.slots;
            var durationValue  = slots.Duration.value;
            var duration       = durationValue is null ? DateTime.Now.AddDays(7) : DateTimeDurationSerializer.GetMaxPremiereDate(durationValue);
            
            var result = await ServerQuery.Instance.GetUpComingTvAsync(duration);

            //switch (Session.alexaSessionDisplayType)
            //{
            //    case AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE:
            //        {
                        var documentTemplateInfo = new RenderDocumentQuery()
                        {
                            baseItems = result.Items.ToList(),
                            renderDocumentType = RenderDocumentType.ITEM_LIST_SEQUENCE_TEMPLATE,
                            HeaderTitle = "Upcoming Episode"
                        };

                        var audioInfo = new AudioDirectiveQuery()
                        {
                            speechContent = SpeechContent.UP_COMING_EPISODES,
                            session = Session,
                            items = result.Items.ToList(),
                            args = new []{duration.ToLongDateString()},
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        };

                        AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

                        var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
                        var renderAudioDirective    = await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(audioInfo);

                        return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                        {
                            shouldEndSession = null,
                            directives = new List<IDirective>()
                            {
                                renderDocumentDirective,
                                renderAudioDirective
                            }

                        }, Session);
                   
        }
    }
}

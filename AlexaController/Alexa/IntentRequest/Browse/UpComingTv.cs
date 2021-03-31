using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.EmbyAplDataSourceManagement;
using AlexaController.EmbyAplDataSourceManagement.PropertyModels;
using AlexaController.EmbyAplManagement;
using AlexaController.Session;
using AlexaController.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    // ReSharper disable once UnusedType.Global
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
            var request = AlexaRequest.request;
            var slots = request.intent.slots;
            var durationValue = slots.Duration.value;
            var duration = durationValue is null ? DateTime.Now.AddDays(7) : DateTimeDurationSerializer.GetMaxPremiereDate(durationValue);

            var result = await ServerQuery.Instance.GetUpComingTvAsync(duration);

            //IDataSource aplDataSource;
            //IDataSource aplaDataSource;

            var sequenceLayoutProperties = await DataSourcePropertiesManager.Instance.GetSequenceViewPropertiesAsync(result.Items.ToList());
            var aplaDataSource = await DataSourcePropertiesManager.Instance.GetSpeechResponseProperties(new SpeechResponsePropertiesQuery()
            {
                SpeechResponseType = SpeechResponseType.UpComingEpisodes, 
                items = result.Items.ToList(), 
                date= duration
            });

            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveFactory.Instance.GetAudioDirectiveAsync(aplaDataSource),
                    await RenderDocumentDirectiveFactory.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(sequenceLayoutProperties, Session)
                }

            }, Session);

        }
    }
}

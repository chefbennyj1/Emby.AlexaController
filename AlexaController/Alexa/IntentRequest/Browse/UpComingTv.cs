using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.DataSourceManagers.DataSourceProperties;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using AlexaController.Utils;

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
            var request       = AlexaRequest.request;
            var slots         = request.intent.slots;
            var durationValue = slots.Duration.value;
            var duration      = durationValue is null ? DateTime.Now.AddDays(7) : DateTimeDurationSerializer.GetMaxPremiereDate(durationValue);

            var result = await ServerQuery.Instance.GetUpComingTvAsync(duration);

            IDataSource aplDataSource;
            IDataSource aplaDataSource;

            aplDataSource  = await AplObjectDataSourceManager.Instance.GetSequenceItemsDataSourceAsync(result.Items.ToList(), null);
            aplaDataSource = await AplAudioDataSourceManager.Instance.UpComingEpisodes(result.Items.ToList(), duration);

            AlexaSessionManager.Instance.UpdateSession(Session, aplDataSource);
            
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                directives = new List<IDirective>()
                {
                    await AplaRenderDocumentDirectiveManager.Instance.GetAudioDirectiveAsync(aplaDataSource),
                    await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<MediaItem>(aplDataSource, Session)
                }

            }, Session);

        }
    }
}

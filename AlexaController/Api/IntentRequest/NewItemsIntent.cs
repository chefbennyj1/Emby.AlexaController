using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using AlexaController.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class NewItemsIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public NewItemsIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var request = AlexaRequest.request;
            var slots = request.intent.slots;
            var duration = slots.Duration.value;
            var type = slots.MovieAlternatives.value is null ? "Series" : "Movie";

            // Default will be 25 days ago unless given a time duration
            var d = duration is null ? DateTime.Now.AddDays(-25) : DateTimeDurationSerializer.GetMinDateCreation(duration);

            var query = type == "Series"
                ? ServerDataQuery.Instance.GetLatestTv(Session.User, d)
                : ServerDataQuery.Instance.GetLatestMovies(Session.User, d);

            var results = query.Where(item => item.IsParentalAllowed(Session.User)).ToList();

            if (!results.Any())
            {
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"No new { type } have been added."
                    },
                    shouldEndSession = true,

                }, Session);
            }

            var sequenceLayoutProperties = await DataSourcePropertiesManager.Instance.GetBaseItemCollectionSequenceViewPropertiesAsync(results);
            var newLibraryItemsAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.NewLibraryItems,
                items = results,
                session = Session,
                date = d
            });

            AlexaSessionManager.Instance.UpdateSession(Session, sequenceLayoutProperties);

            var renderDocumentDirective = await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(sequenceLayoutProperties, Session);
            var renderAudioDirective = await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(newLibraryItemsAudioProperties);

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

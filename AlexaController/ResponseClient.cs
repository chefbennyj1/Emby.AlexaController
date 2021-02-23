using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Alexa.SpeechSynthesisMarkupLanguage;
using AlexaController.Session;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;

namespace AlexaController
{
    public class AlexaResponse
    { 
        public string version                                              { get; set; }
        public IResponse response                                          { get; set; }
    }

    public interface IResponseClient
    {
        Task<string> BuildAlexaResponse(IResponse response,IAlexaSession session);
        Task PostProgressiveResponse(string speechOutput, string accessToken, string requestId);
    }

    public class ResponseClient : IResponseClient
    {
        private IJsonSerializer JsonSerializer { get; }
        private IHttpClient HttpClient         { get; }
        public static IResponseClient Instance { get; private set; }

        public ResponseClient(IJsonSerializer jsonSerializer, IHttpClient client)
        {
            JsonSerializer = jsonSerializer;
            HttpClient     = client;
            Instance       = this;
        }

        // ReSharper disable once FlagArgument
        public async Task<string> BuildAlexaResponse(IResponse response, IAlexaSession session)
        {
            // ReSharper disable once ComplexConditionExpression
            var person = !(session.person is null) && response.SpeakUserName ? Ssml.SayName(session.person) : "";
            
            if (!(response.outputSpeech is null))
            {
                var outputSpeech = response.outputSpeech;

                var speech = new StringBuilder();
               
                speech.Append(outputSpeech.sound);
                speech.Append(person);
                speech.Append(Ssml.InsertStrengthBreak(StrengthBreak.strong));
                speech.Append(outputSpeech.phrase);
                
                outputSpeech.ssml = "<speak>";
                outputSpeech.ssml += speech.ToString();
                outputSpeech.ssml += "</speak>";
            }

            response.reprompt = new Reprompt
            {
                outputSpeech = new OutputSpeech() { ssml = "<speak>Can I help you with anything? You can ask to show a movie, or to show a tv series.</speak>" }
            };

            // Remove the directive if the device doesn't handle APL.
            if (!session.supportsApl)
            {
                if (response.directives.Any(d => d.type == "Alexa.Presentation.APL.RenderDocument"))
                {
                    response.directives.RemoveAll(d => d.type == "Alexa.Presentation.APL.RenderDocument");
                }
            } 

            return await Task.FromResult(JsonSerializer.SerializeToString(new AlexaResponse()
            {
                version = "1.2",
                response = response
            }));
        }

        public async Task PostProgressiveResponse(string speechOutput, string accessToken, string requestId)
        {
            var response = new Response
            {
                header = new Header() { requestId = requestId },
                directive = new Directive()
                {
                    speech = $"<speak>{speechOutput}</speak>",
                    type   = "VoicePlayer.Speak"
                }
            };

            var json = JsonSerializer.SerializeToString(response);
            var options = new HttpRequestOptions
            {
                Url                = "https://api.amazonalexa.com/v1/directives",
                RequestContentType = "application/json",
                RequestContent     = json.ToCharArray(),
                RequestHeaders     = { ["Authorization"] = "Bearer " + accessToken }
            };
            
            await HttpClient.SendAsync(options, "POST").ConfigureAwait(false);
        }

    }
}
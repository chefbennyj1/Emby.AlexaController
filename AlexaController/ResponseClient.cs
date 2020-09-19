using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Serialization;


namespace AlexaController
{

    public class AlexaResponse
    { 
        public string version                                              { get; set; }
        public IResponse response                                          { get; set; }
        public string userAgent                                            { get; set; }
    }

    public interface IResponseClient
    {
        string BuildAlexaResponse(IResponse response, AlexaSessionDisplayType alexaSessionDisplayType = AlexaSessionDisplayType.NONE);
        void PostProgressiveResponse(string speechOutput, string accessToken, string requestId, string dialog = "");
    }

    public class ResponseClient : IResponseClient
    {
        private IJsonSerializer JsonSerializer { get; }
        private IHttpClient HttpClient         { get; }
        public static IResponseClient Instance { get; set; }
        public ResponseClient(IJsonSerializer jsonSerializer, IHttpClient client)
        {
            JsonSerializer = jsonSerializer;
            HttpClient     = client;
            Instance = this;
        }

        public string BuildAlexaResponse(IResponse response, AlexaSessionDisplayType alexaSessionDisplayType = AlexaSessionDisplayType.NONE)
        {
            var person = !(response.person is null) ? OutputSpeech.SayName(response.person) : "";
            
            if (!(response.outputSpeech is null))
            {
                var outputSpeech = response.outputSpeech;

                var speech       = string.Empty;

                speech += outputSpeech.sound;
                speech += SemanticSpeechUtility.GetSemanticSpeechResponse(outputSpeech.semanticSpeechType);
                speech += person;
                speech += OutputSpeech.InsertStrengthBreak(StrengthBreak.strong);
                speech += outputSpeech.phrase;


                outputSpeech.ssml = "<speak>";
                outputSpeech.ssml += speech;
                outputSpeech.ssml += "</speak>";

            }

            // Remove the directive if the device doesn't handle APL.
            if (!alexaSessionDisplayType.Equals(AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE)) response.directives = null;
            
            return JsonSerializer.SerializeToString(new AlexaResponse()
            {
                version = "1.2",
                response = response
            });
        }

        public void PostProgressiveResponse(string speechOutput, string accessToken, string requestId, string dialog = "")
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

            var options = new HttpRequestOptions
            {
                Url                = "https://api.amazonalexa.com/v1/directives",
                RequestContentType = "application/json",
                RequestContent     = JsonSerializer.SerializeToString(response).ToCharArray(),
                RequestHeaders     = { ["Authorization"] = "Bearer " + accessToken }
            };
            
            HttpClient.SendAsync(options, "POST");

        }

    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils.LexicalSpeech;


namespace AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page
{
    public class HelpPager : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }

        public HelpPager(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
        }

        public async Task<string> Response()
        {
            var request       = AlexaRequest.request;
            var arguments     = request.arguments;
            var session  = AlexaSessionManager.Instance.GetSession(AlexaRequest);
            var helpListIndex = Convert.ToInt32(arguments[2]);
            session.person = null;
            ServerQuery.Instance.Log.Info("ALEXA HELP PAGES " + helpListIndex);
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.HelpStrings.ElementAt(helpListIndex)
                }

            }, session);
        }
    }
}

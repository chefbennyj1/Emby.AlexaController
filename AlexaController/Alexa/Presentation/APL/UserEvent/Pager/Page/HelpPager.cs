using System;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;


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
            var helpListIndex = Convert.ToInt32(arguments[2]) -1;
            session.person = null;
            ServerController.Instance.Log.Info("ALEXA HELP PAGES " + helpListIndex);
            return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = null,
                outputSpeech = new OutputSpeech()
                {
                    phrase = helpListIndex > 0 ? RenderAudioManager.HelpStrings.ElementAt(helpListIndex) : ""
                }

            }, session);
        }
    }
}

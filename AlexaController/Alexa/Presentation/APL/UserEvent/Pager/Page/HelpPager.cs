using System;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Presentation.APL.UserEvent.Pager.Page
{
    public class HelpPager : IUserEventResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        

        public HelpPager(IAlexaRequest alexaRequest)
        {
            AlexaRequest = alexaRequest;
            ;
        }
        public async Task<string> Response()
        {
            var request       = AlexaRequest.request;
            var arguments     = request.arguments;
            var helpListIndex = Convert.ToInt32(arguments[2]);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = null,
                outputSpeech = new OutputSpeech()
                {
                    phrase = SpeechStrings.HelpStrings.ElementAt(helpListIndex)
                },

            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE).Result;
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Session;

// ReSharper disable once ComplexConditionExpression
// ReSharper disable once TooManyArguments

namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
    public class StreamingUsersIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        

        public StreamingUsersIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var speechString = GetUserSessionSpeechString(EmbyServerEntryPoint.Instance.GetCurrentSessions());

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = speechString,
                    
                }
            }, Session);
        }

        private static string GetUserSessionSpeechString(IEnumerable<SessionInfo> sessions)
        {
            
            sessions = sessions.Where(session => !string.IsNullOrEmpty(session.UserName));

            var sessionInfos = sessions.ToList();

            if (!sessionInfos.Any()) return "There is currently no one using unity home theater services at this time.";

            
            var speech = new StringBuilder();
            speech.Append("There ");
            speech.Append(sessionInfos.Count > 1 ? "are ": "is ");
            speech.Append("currently ");
            speech.Append(sessionInfos.Count);
            speech.Append(sessionInfos.Count > 1 ? "sessions" : "session");
            speech.Append(" active on the server.");

            return speech.ToString();
            //return sessionInfos.Where(session => !string.IsNullOrEmpty(session.UserName))
            //    .Aggregate(s, (current, session) => !current.Contains(session.UserName) //Don't duplicate the user message with two sessions
            //        ? current + $"{session.UserName} has {sessionInfos.Count(ses => ses.UserName == session.UserName)} open " +
            //          $"{ (sessionInfos.Count() > 1 ? "sessions" : "session") }. " : current + string.Empty);
        }
    }
}

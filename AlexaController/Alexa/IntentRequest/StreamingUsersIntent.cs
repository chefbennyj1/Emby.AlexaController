using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.RequestData.Model;
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

namespace AlexaController.Alexa.IntentRequest
{
    [Intent]
    public class StreamingUsersIntent : IntentResponseModel
    {
        public override string Response
        (AlexaRequest alexaRequest, AlexaSession session, IResponseClient responseClient, ILibraryManager libraryManager, ISessionManager sessionManager, IUserManager userManager)
        {
            var speechString = GetUserSessionSpeechString(sessionManager.Sessions);

            return responseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = speechString,
                    semanticSpeechType = SemanticSpeechType.COMPLIANCE
                }
            }, session.alexaSessionDisplayType);
        }

        private static string GetUserSessionSpeechString(IEnumerable<SessionInfo> sessions)
        {
            
            sessions = sessions.Where(session => !string.IsNullOrEmpty(session.UserName));

            var sessionInfos = sessions.ToList();

            if (!sessionInfos.Any()) return "There is currently no one using unity home theater services at this time.";

            var s = string.Empty;
            s += $"There { (sessionInfos.Count() > 1 ? "are" : "is") } " +
                 $" currently {sessionInfos.Count()} { (sessionInfos.Count() > 1 ? "sessions" : "session") } active on the server. ";

            return sessionInfos.Where(session => !string.IsNullOrEmpty(session.UserName))
                .Aggregate(s, (current, session) => !current.Contains(session.UserName) //Don't duplicate the user message with two sessions
                    ? current + $"{session.UserName} has {sessionInfos.Count(ses => ses.UserName == session.UserName)} open " +
                      $"{ (sessionInfos.Count() > 1 ? "sessions" : "session") }. " : current + string.Empty);
        }
    }
}

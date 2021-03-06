﻿using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Session;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class StreamingUsersIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }


        public StreamingUsersIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var speechString = GetUserSessionSpeechString(ServerDataQuery.Instance.GetCurrentSessions());

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
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
            speech.Append(sessionInfos.Count > 1 ? "are " : "is ");
            speech.Append("currently ");
            speech.Append(sessionInfos.Count);
            speech.Append(sessionInfos.Count > 1 ? "sessions" : "session");
            speech.Append(" active on the server.");

            return speech.ToString();

        }
    }
}

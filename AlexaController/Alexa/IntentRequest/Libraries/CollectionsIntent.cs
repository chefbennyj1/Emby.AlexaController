﻿using System.Threading.Tasks;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Api;
using AlexaController.Session;

namespace AlexaController.Alexa.IntentRequest.Libraries
{
    [Intent]
    public class CollectionsIntent  : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public CollectionsIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            return await new LibraryIntentResponse("Collections").Response(AlexaRequest, Session);
        }
    }
}

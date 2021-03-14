﻿using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.DataSourceManagers;
using AlexaController.PresentationManagers;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;
using Value = AlexaController.DataSourceManagers.DataSourceProperties.Value;

namespace AlexaController.Alexa.IntentRequest.AMAZON
{
    [Intent]
    // ReSharper disable once UnusedType.Global
    public class HelpIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }

        public HelpIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            var dataSource = await AplObjectDataSourceManager.Instance.GetHelpDataSourceAsync();
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Welcome to home theater help. In a moment, swipe left and follow the help instructions.",
                },
                shouldEndSession = null,
                SpeakUserName = true,
                directives = new List<IDirective>()
                {
                    await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<List<Value>>(dataSource, Session)
                }
            }, Session);
        }
    }
}

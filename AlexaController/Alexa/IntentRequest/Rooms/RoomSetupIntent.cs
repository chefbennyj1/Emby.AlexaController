﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.Presentation.DataSources.Properties;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Api;
using AlexaController.Session;


namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomSetupIntent : IntentResponseBase<IAlexaRequest, IAlexaSession>, IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public RoomSetupIntent(IAlexaRequest alexaRequest, IAlexaSession session) : base(alexaRequest, session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }

        public async Task<string> Response()
        {
            var room = Session.room;

            if (room is null)
            {
                Session.PersistedRequestContextData = AlexaRequest;

                var dataSource =
                    await AplDataSourceManager.Instance.GetGenericViewDataSource("Please say the name of the room you want to setup.", "/particles");

                AlexaSessionManager.Instance.UpdateSession(Session, null);



                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<IDirective>()
                    {
                        await AplRenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync<IProperty>(dataSource, Session)
                    }

                }, Session);
            }
            
            var response = await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { room.Name }, and press the \"Create Room button\".",

                }
            }, Session);

            Session.room = null;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            return response;
        }
    }
}

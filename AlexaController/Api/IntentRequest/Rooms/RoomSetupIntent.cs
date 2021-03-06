﻿using AlexaController.Alexa;
using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlexaController.Api.IntentRequest.Rooms
{
    [Intent]
    // ReSharper disable once UnusedType.Global
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
                Session.PersistedRequestData = AlexaRequest;

                var genericLayoutProperties =
                    await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync("Please say the name of the room you want to setup.", "/particles");

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
                        await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync<string>(genericLayoutProperties, Session)
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

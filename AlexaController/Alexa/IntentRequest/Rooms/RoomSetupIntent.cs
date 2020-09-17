﻿using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using MediaBrowser.Model.Services;


// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.IntentRequest.Rooms
{
    [Intent]
    public class RoomSetupIntent : IIntentResponse, IService
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        public IAlexaEntryPoint Alexa { get; }

        public RoomSetupIntent(IAlexaRequest alexaRequest, IAlexaSession session, IAlexaEntryPoint alexa)
        {
            AlexaRequest = alexaRequest;
            Alexa = alexa;
            Session = session;
            Alexa = alexa;
        }
        public string Response()
        {
            var room = Session.room;
            if (room == null)
            {
                Session.PersistedRequestData = AlexaRequest;
                AlexaSessionManager.Instance.UpdateSession(Session);

                return Alexa.ResponseClient.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = "Welcome to room setup. Please say the name of the room you want to setup."
                    },
                    shouldEndSession = false,
                    directives = new List<IDirective>()
                    {
                        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                        {
                            renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                            HeadlinePrimaryText = "Please say the name of the room you want to setup.",

                        }, Session)
                    }

                }, Session.alexaSessionDisplayType);
            }
            
            var response = Alexa.ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = $"Thank you. Please see the plugin configuration to choose the emby device that is in the { room.Name }, and press the \"Create Room button\".",

                }
            }, Session.alexaSessionDisplayType);

            Session.room = null;

            return response;
        }
    }
}

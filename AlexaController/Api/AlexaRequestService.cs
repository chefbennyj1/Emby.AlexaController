﻿using System;
using System.Collections.Generic;
using MediaBrowser.Model.Services;
using System.IO;
using System.Threading.Tasks;
using AlexaController.Alexa;
using AlexaController.Alexa.IntentRequest;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Serialization;

// ReSharper disable once TooManyDependencies
// ReSharper disable once PossibleNullReferenceException

namespace AlexaController.Api
{
    public interface IAlexaRequest
    {
        AmazonSession session { get; set; }
        Request request       { get; set; }
        Context context       { get; set; }
        string version        { get; set; }
        Event @event          { get; set; }
    }

    [Route("/Alexa", "POST", Summary = "Alexa End Point")]
    public class AlexaRequest : IRequiresRequestStream, IAlexaRequest
    {
        public Stream RequestStream  { get; set; }
        public AmazonSession session { get; set; }
        public Request request       { get; set; }
        public Context context       { get; set; }
        public string version        { get; set; }
        public Event @event          { get; set; }
    }
    
    public class AlexaRequestService : IService
    {
        private IJsonSerializer JsonSerializer { get; }
        
        private readonly Func<Intent, bool> IsVoiceAuthenticationAccountLinkRequest = intent  => intent.name == "VoiceAuthenticationAccountLink";
        private readonly Func<Intent, bool> IsRoomNameIntentRequest                 = intent  => intent.name == "Rooms_RoomNameIntent";
        
        
        public AlexaRequestService(IJsonSerializer json, IHttpClient client, IUserManager user, ISessionManager sessionManager)
        {
            JsonSerializer = json;
            
            if (AlexaResponseClient.Instance is null)
                Activator.CreateInstance(typeof(AlexaResponseClient), json, client);

            if (AlexaSessionManager.Instance is null)
                Activator.CreateInstance(typeof(AlexaSessionManager), sessionManager);

            if (RoomManager.Instance is null)
                Activator.CreateInstance<RoomManager>();

            if (SpeechAuthorization.Instance is null)
                Activator.CreateInstance(typeof(SpeechAuthorization), user);

            if (RenderDocumentDirectiveManager.Instance is null)
                Activator.CreateInstance<RenderDocumentDirectiveManager>();

            if (AudioDirectiveManager.Instance is null)
                Activator.CreateInstance<AudioDirectiveManager>();

            if (DataSourceManager.Instance is null)
                Activator.CreateInstance<DataSourceManager>();
        }

        public async Task<object> Post(AlexaRequest data)
        {
            using (var sr = new StreamReader(data.RequestStream))
            {
                var s = await sr.ReadToEndAsync();
                var alexaRequest = JsonSerializer.DeserializeFromString<AlexaRequest>(s);

                ServerController.Instance.Log.Info($"Alexa incoming request: {alexaRequest.request.type}");
                
                switch (alexaRequest.request.type)
                {
                    case "Alexa.Presentation.APL.UserEvent"    : return await OnUserEvent(alexaRequest);
                    case "IntentRequest"                       : return await OnIntentRequest(alexaRequest);
                    case "SessionEndedRequest"                 : return await OnSessionEndRequest(alexaRequest);
                    case "LaunchRequest"                       : return await OnLaunchRequest(alexaRequest);
                    case "System.ExceptionEncountered"         : return await OnExceptionEncountered();
                    default                                    : return await OnDefault();
                }
            }
        }


        private async Task<string> OnExceptionEncountered()
        {
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech     = new OutputSpeech(){phrase = "I have encountered an error."},
                shouldEndSession = true,
                SpeakUserName    = true
            }, null);
        }

        private async Task<string> OnIntentRequest(IAlexaRequest alexaRequest)
        {
            IAlexaSession session = null;
            
            var request = alexaRequest.request;
            var intent  = request.intent;
            var context = alexaRequest.context;
            var system  = context.System;
            var person  = system.person;

            if (!IsVoiceAuthenticationAccountLinkRequest(intent)) // create a session
            {
                if (!(person is null))
                {
                    if (!SpeechAuthorization.Instance.UserPersonalizationProfileExists(person))
                        
                        return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                        {
                            shouldEndSession = true,
                            outputSpeech = new OutputSpeech()
                            {
                                phrase = "You are not a recognized user. Please take moment to register your voice profile.",
                                
                            }

                        }, null);
                }

                var user = SpeechAuthorization.Instance.GetRecognizedPersonalizationProfileResult(person);

                session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);
               
                //How can there be a room intent request without any session context data? There can not be.
                if (session.PersistedRequestContextData is null && IsRoomNameIntentRequest(intent))
                {
                    //end the session.
                    return await new NotUnderstood(alexaRequest, session).Response(); 
                }
            }
            
            try
            {
                //Amazon Alexa Custom SKill Console does not allow "." in skill names.
                //This would make creating namespace paths easier.
                //Instead we save the skill name with "_", which replaces the "." in the reflected path to the corresponding .cs file.
                //Replace the "_" (underscore) with a "." (period) to create the proper reflection path to the corresponding IntentRequest file.
                var intentName = intent.name.Replace("_", ".");

                return await GetResponseResult( Type.GetType($"AlexaController.Alexa.IntentRequest.{intentName}"), alexaRequest, session);
            }
            catch (Exception exception)
            {
                var dataSource = await DataSourceManager.Instance.GetGenericHeadline(exception.Message);
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"{Ssml.SayWithEmotion($"Sorry, I was unable to do that. {exception.Message}", Emotion.excited, Intensity.low)}",
                    },

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance
                            .GetRenderDocumentDirectiveAsync(dataSource, session)
                    }
                }, session);
            }
        }

        private static async Task<string> OnSessionEndRequest(IAlexaRequest alexaRequest)
        {
            await Task.Run(() => AlexaSessionManager.Instance.EndSession(alexaRequest));
            return null;
        }
        
        private async Task<string> OnUserEvent(IAlexaRequest alexaRequest)
        {
            var request    = alexaRequest.request;
            ServerController.Instance.Log.Info($"USEREVENT: {alexaRequest.request.source.type} {alexaRequest.request.source.handler}");
            return await GetResponseResult(Type.GetType($"AlexaController.Alexa.Presentation.APL.UserEvent.{request.source.type}.{request.source.handler}.{request.arguments[0]}"), alexaRequest, null);
        }

        private static async Task<string> OnLaunchRequest(IAlexaRequest alexaRequest)
        {
            var context = alexaRequest.context;
            
            var user = SpeechAuthorization.Instance.GetRecognizedPersonalizationProfileResult(context.System.person);
            
            if (user is null)
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(
                    new Response()
                    {
                        shouldEndSession = true,
                        SpeakUserName = true,
                        directives = new List<IDirective>()
                        {
                            await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(new AudioDirectiveQuery()
                            {
                                speechContent = SpeechContent.PARENTAL_CONTROL_NOT_ALLOWED,
                                audio = new Audio()
                                {
                                    source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                    
                                }
                            })
                        }
                    }, null);

            var session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);
            var dataSource =
                await DataSourceManager.Instance.GetGenericHeadline("Welcome to Home Theater Emby Controller");
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                SpeakUserName = true,
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(dataSource, session),
                    await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(new AudioDirectiveQuery()
                    {
                        speechContent = SpeechContent.ON_LAUNCH,
                        session = session,
                        audio = new Audio()
                        {
                            source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                            
                        }
                    })
                }
            }, session);
        }

        private static async Task<string> OnDefault()
        {
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Unknown"
                }
            }, null);
        }

        private static async Task<string> GetResponseResult(Type type, IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var paramArgs = session is null
                ? new object[] { alexaRequest } : new object[] { alexaRequest, session };
            
            var instance = Activator.CreateInstance(type, paramArgs);
            return await (Task<string>)type.GetMethod("Response").Invoke(instance, null);

        }
    }
}




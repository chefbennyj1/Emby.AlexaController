using System;
using System.Collections.Generic;
using MediaBrowser.Model.Services;
using System.IO;
using AlexaController.Alexa.Errors;
using AlexaController.Alexa.IntentRequest;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Serialization;


// ReSharper disable once TooManyDependencies

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
        private IUserManager UserManager       { get; }
        
        private readonly Func<Intent, bool> IsVoiceAuthenticationAccountLinkRequest = intent => intent.name == "VoiceAuthenticationAccountLink";
        private readonly Func<Intent, bool> IsRoomNameIntentRequest = intent => intent.name == "Rooms_RoomNameIntent";

        private readonly Func<Request, string> IntentNamespace    = request => $"AlexaController.Alexa.IntentRequest.{request.intent.name.Replace("_", ".")}";
        private readonly Func<Request, string> UserEventNamespace = request => $"AlexaController.{request.type}.{request.source.type}.{request.source.handler}.{request.arguments[0]}";
        
        public AlexaRequestService(IJsonSerializer json, IHttpClient client, IUserManager user)
        {
            JsonSerializer = json;
            UserManager    = user;
            if(RoomContextManager.Instance is null)
                Activator.CreateInstance<RoomContextManager>();
            if(ResponseClient.Instance is null)
                Activator.CreateInstance(typeof(ResponseClient), json, client);
            //Activator.CreateInstance(typeof(AlexaEntryPoint), json, client);
        }

        public object Post(AlexaRequest data)
        {
            var alexaRequest = JsonSerializer.DeserializeFromStream<AlexaRequest>(data.RequestStream);

            switch (alexaRequest.request.type)
            {
                case "Alexa.Presentation.APL.UserEvent" : return OnUserEvent(alexaRequest);
                case "IntentRequest"                    : return OnIntentRequest(alexaRequest);
                case "SessionEndedRequest"              : return OnSessionEndRequest(alexaRequest);
                case "LaunchRequest"                    : return OnLaunchRequest(alexaRequest);
                default                                 : return OnDefault();
            }
        }

        private string OnIntentRequest(IAlexaRequest alexaRequest)
        {
            IAlexaSession session = null;
            
            var request = alexaRequest.request;
            var intent  = request.intent;
            var context = alexaRequest.context;
            var system  = context.System;
            var person  = system.person;
            

            if (!IsVoiceAuthenticationAccountLinkRequest(intent)) //is not voice training, create a session
            {
                if (!(person is null))
                {
                    if (!SpeechAuthorization.Instance.UserPersonalizationProfileExists(person))
                        return ResponseClient.Instance.BuildAlexaResponse(new Response()
                        {
                            shouldEndSession = true,
                            outputSpeech = new OutputSpeech()
                            {
                                phrase = "You are not a recognized user. Please take moment to register your voice profile.",
                                semanticSpeechType = SemanticSpeechType.APOLOGETIC
                            },
                        });
                }

                var user = SpeechAuthorization.Instance.GetRecognizedPersonalizationProfileResult(person);

                session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);
                
                //The "RoomName" intent will always be used during follow up communication with Alexa.
                //If Alexa thinks she has heard a "RoomName" intent request, without the session having any PersistedRequestData
                //There has been a mistake, end the session.
                if (session.PersistedRequestData is null && IsRoomNameIntentRequest(intent))
                {
                    return new NotUnderstood(alexaRequest, session).Response(); 
                }

            }
            
            var type = Type.GetType(IntentNamespace(request));

            try
            {
                return GetResponseResult(type, alexaRequest, session); 
            }
            catch (Exception exception)
            {
                return new ErrorHandler().OnError(new Exception($"I was unable to do that. Please try again. {exception.Message}"), alexaRequest, session, ResponseClient.Instance);
            }
        }

        private static string OnSessionEndRequest(IAlexaRequest alexaRequest)
        {
            AlexaSessionManager.Instance.EndSession(alexaRequest);
            return null;
        }

        
        private string OnUserEvent(IAlexaRequest alexaRequest)
        {
            var request = alexaRequest.request;
            var type = Type.GetType(UserEventNamespace(request));
            return GetResponseResult(type, alexaRequest, null); 
        }

        private string OnLaunchRequest(IAlexaRequest alexaRequest)
        {
            var context             = alexaRequest.context;

            //check the person speaking and get their Emby account UserName
            var speechAuthorization = new SpeechAuthorization(UserManager);

            var user   = speechAuthorization.GetRecognizedPersonalizationProfileResult(context.System.person);
            var person = !ReferenceEquals(null, context.System.person) ? OutputSpeech.SayName(context.System.person) : "";

            if (user is null)
                return ResponseClient.Instance.BuildAlexaResponse(
                    new Response()
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = "I don't recognize the current user. Please go to the plugin configuration and link emby account personalization. Or ask for help.",
                            semanticSpeechType = SemanticSpeechType.APOLOGETIC
                        },
                    });

            var session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);

            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = "<audio src=\"soundbank://soundlibrary/alarms/beeps_and_bloops/intro_02\"/>" +
                             person + $"{OutputSpeech.InsertStrengthBreak(StrengthBreak.strong)} " +
                             "What media can I help you find.",
                    semanticSpeechType = SemanticSpeechType.GREETINGS
                },
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplate()
                    {
                        HeadlinePrimaryText = "Welcome to Home Theater Emby Controller",
                        renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                    }, session)

                }

            }, session.alexaSessionDisplayType);

        }

        private string OnDefault()
        {
            return ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Unknown"
                }
            });
        }

        private static string GetResponseResult(Type @namespace, IAlexaRequest alexaRequest, IAlexaSession session)
        {
            var paramsArgs = session is null
                ?  new object[] { alexaRequest }
                :  new object[] { alexaRequest, session };

            var instance = Activator.CreateInstance(@namespace, paramsArgs);
            return (string)@namespace.GetMethod("Response")?.Invoke(instance, null);
        }
       
    }
}




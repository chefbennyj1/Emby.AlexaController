using System;
using System.Collections.Generic;
using MediaBrowser.Model.Services;
using System.IO;
using AlexaController.Alexa.Errors;
using AlexaController.Alexa.IntentRequest;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Serialization;

// ReSharper disable TooManyChainedReferences
// ReSharper disable MethodNameNotMeaningful
// ReSharper disable InconsistentNaming
// ReSharper disable once CollectionNeverUpdated.Global
// ReSharper disable once TooManyDependencies

namespace AlexaController.Api
{
    [Route("/Alexa", "POST", Summary = "Alexa End Point")]
    public class AlexaRequest : IRequiresRequestStream
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
        private ISessionManager SessionManager { get; }
        private ILibraryManager LibraryManager { get; }
        private IResponseClient ResponseClient { get; }

        private readonly Func<Intent, bool> IsVoiceAuthenticationAccountLinkRequest = intent => intent.name == "VoiceAuthenticationAccountLink";
        private readonly Func<Intent, bool> IsRoomNameRequest                       = intent => intent.name == "Rooms_RoomNameIntent";

        private readonly Func<Request, string> IntentNamespace    = request => $"AlexaController.Alexa.IntentRequest.{request.intent.name.Replace("_", ".")}";
        private readonly Func<Request, string> UserEventNamespace = request => $"AlexaController.{request.type}.{request.source.type}.{request.source.handler}.{request.arguments[0]}";
        
        public AlexaRequestService
        (IJsonSerializer json, IHttpClient client, IUserManager user, ISessionManager session, ILibraryManager library)
        {
            JsonSerializer = json;
            UserManager    = user;
            SessionManager = session;
            LibraryManager = library;
            ResponseClient = new ResponseClient(json, client);
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

        private string OnIntentRequest(AlexaRequest alexaRequest)
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
                        return ResponseClient.BuildAlexaResponse(new Response()
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
                if (session.PersistedRequestData is null && IsRoomNameRequest(intent))
                {
                    return new NotUnderstood().Response(alexaRequest, session, ResponseClient, LibraryManager, SessionManager, UserManager);
                }

            }
            
            var requestHandlerParams = new object[] { alexaRequest, session, ResponseClient, LibraryManager, SessionManager, UserManager };
            var type                 = Type.GetType(IntentNamespace(request));

            try
            {
                return GetResponseResult(type, requestHandlerParams);
            }
            catch (Exception exception)
            {
                return ErrorHandler.Instance.OnError(new Exception($"I was unable to do that. Please try again. {exception.Message}"), alexaRequest, session, ResponseClient);
            }
        }

        private static string OnSessionEndRequest(AlexaRequest alexaRequest)
        {
            AlexaSessionManager.Instance.EndSession(alexaRequest);
            return null;
        }

        private string OnUserEvent(AlexaRequest alexaRequest)
        {
            var request              = alexaRequest.request;
            var requestHandlerParams = new object[] { alexaRequest, LibraryManager, ResponseClient, SessionManager };
            var type                 = Type.GetType(UserEventNamespace(request));
            return GetResponseResult(type, requestHandlerParams);
        }

        private string OnLaunchRequest(AlexaRequest alexaRequest)
        {
            var context             = alexaRequest.context;

            //check the person speaking and get their Emby account UserName
            var speechAuthorization = new SpeechAuthorization(UserManager);

            var user   = speechAuthorization.GetRecognizedPersonalizationProfileResult(context.System.person);
            var person = !ReferenceEquals(null, context.System.person) ? OutputSpeech.SayName(context.System.person) : "";

            if (user is null)
                return ResponseClient.BuildAlexaResponse(
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

            return ResponseClient.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    phrase = "<audio src=\"soundbank://soundlibrary/alarms/beeps_and_bloops/intro_02\"/>" +
                             person + $"{OutputSpeech.InsertStrengthBreak(StrengthBreak.strong)} " +
                             "What media can I help you find.",
                    semanticSpeechType = SemanticSpeechType.GREETINGS
                },
                shouldEndSession = false,
                directives = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                    {
                        HeadlinePrimaryText = "Welcome to Home Theater Emby Controller",
                        renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,

                    }, session)

                }

            }, session.alexaSessionDisplayType);

        }

        private string OnDefault()
        {
            return ResponseClient.BuildAlexaResponse(new Response()
            {
                shouldEndSession = true,
                outputSpeech = new OutputSpeech()
                {
                    phrase = "Unknown"
                }
            });
        }

        private string GetResponseResult(Type @namespace, object[] requestHandlerParams)
        {
            var instance = Activator.CreateInstance(@namespace ?? throw new Exception("Error getting response"));
            var method   = @namespace.GetMethod("Response");
            var response = method?.Invoke(instance, requestHandlerParams);
            return (string)response; 
        }

       
    }
}




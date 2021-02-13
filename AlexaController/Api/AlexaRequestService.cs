using System;
using System.Collections.Generic;
using MediaBrowser.Model.Services;
using System.IO;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Alexa.SpeechSynthesisMarkupLanguage;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;
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
            
            if (ResponseClient.Instance is null)
                Activator.CreateInstance(typeof(ResponseClient), json, client);

            if (AlexaSessionManager.Instance is null)
                Activator.CreateInstance(typeof(AlexaSessionManager), sessionManager);

            if (RoomManager.Instance is null)
                Activator.CreateInstance<RoomManager>();

            if (SpeechAuthorization.Instance is null)
                Activator.CreateInstance(typeof(SpeechAuthorization), user);

            if (RenderDocumentBuilder.Instance is null)
                Activator.CreateInstance<RenderDocumentBuilder>();
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
                    case "Alexa.Presentation.APL.UserEvent" : return await OnUserEvent(alexaRequest);
                    case "IntentRequest"                    : return await OnIntentRequest(alexaRequest);
                    case "SessionEndedRequest"              : return await OnSessionEndRequest(alexaRequest);
                    case "LaunchRequest"                    : return await OnLaunchRequest(alexaRequest);
                    case "System.ExceptionEncountered"      : return await OnExceptionEncountered();
                    default                                 : return await OnDefault();
                }
            }
        }

      

        private async Task<string> OnExceptionEncountered()
        {
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech(){phrase = "I have encountered an error."},
                shouldEndSession = true,
                SpeakUserName = true
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
                        
                        return await ResponseClient.Instance.BuildAlexaResponse(new Response()
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
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"{Ssml.SayWithEmotion($"Sorry, I was unable to do that. {exception.Message}", Emotion.excited, Intensity.low)}",
                    },

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance
                            .GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                            {
                                renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                                HeadlinePrimaryText = exception.Message

                            }, session)
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
                return await ResponseClient.Instance.BuildAlexaResponse(
                    new Response()
                    {
                        shouldEndSession = true,
                        outputSpeech = new OutputSpeech()
                        {
                            phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                            {
                                type = SpeechResponseType.PERSON_NOT_RECOGNIZED
                            })
                        }, 
                        SpeakUserName = true
                    }, null);

            var session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);

            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
            {
                outputSpeech = new OutputSpeech()
                {
                    sound = "<audio src=\"soundbank://soundlibrary/alarms/beeps_and_bloops/intro_02\"/>",
                    phrase= await SpeechStrings.GetPhrase(new SpeechStringQuery()
                    {
                        type = SpeechResponseType.ON_LAUNCH, 
                        session = session
                    })
                   
                },
                SpeakUserName = true,
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentTemplate()
                    {
                        HeadlinePrimaryText = "Welcome to Home Theater Emby Controller",
                        renderDocumentType  = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE
                    }, session)

                }
            }, session);

        }

        private static async Task<string> OnDefault()
        {
            return await ResponseClient.Instance.BuildAlexaResponse(new Response()
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




using AlexaController.Alexa.RequestModel;
using AlexaController.Alexa.ResponseModel;
using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Api.IntentRequest;
using AlexaController.Api.IntentRequest.Rooms;
using AlexaController.EmbyApl;
using AlexaController.EmbyAplDataSource;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Serialization;
using MediaBrowser.Model.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

// ReSharper disable once TooManyDependencies
// ReSharper disable once PossibleNullReferenceException

namespace AlexaController.Api
{
    public interface IAlexaRequest
    {
        AmazonSession session { get; set; }
        Request request { get; set; }
        Context context { get; set; }
        string version { get; set; }
        Event @event { get; set; }
    }

    [Route("/Alexa", "POST", Summary = "Alexa End Point")]
    public class AlexaRequest : IRequiresRequestStream, IAlexaRequest
    {
        public Stream RequestStream { get; set; }
        public AmazonSession session { get; set; }
        public Request request { get; set; }
        public Context context { get; set; }
        public string version { get; set; }
        public Event @event { get; set; }
    }

    // ReSharper disable once UnusedType.Global
    public class AlexaRequestService : IService
    {
        private IJsonSerializer JsonSerializer { get; }

        private readonly Func<Intent, bool> IsVoiceAuthenticationAccountLinkRequest = intent => intent.name == "VoiceAuthenticationAccountLink";
        private readonly Func<Intent, bool> IsRoomNameIntentRequest = intent => intent.name == "Rooms_RoomNameIntent";

        public AlexaRequestService(IJsonSerializer json, IHttpClient client, IUserManager user, ISessionManager sessionManager)
        {
            JsonSerializer = json;

            if (AlexaResponseClient.Instance is null)
                Activator.CreateInstance(typeof(AlexaResponseClient), json, client);

            if (AlexaSessionManager.Instance is null)
                Activator.CreateInstance(typeof(AlexaSessionManager), sessionManager);

            if (RoomContextManager.Instance is null)
                Activator.CreateInstance<RoomContextManager>();

            if (SpeechAuthorization.Instance is null)
                Activator.CreateInstance(typeof(SpeechAuthorization), user);

            if (RenderDocumentDirectiveManager.Instance is null)
                Activator.CreateInstance<RenderDocumentDirectiveManager>();

            if (DataSourcePropertiesManager.Instance is null)
                Activator.CreateInstance<DataSourcePropertiesManager>();
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
                    case "Alexa.Presentation.APL.UserEvent": return await OnUserEvent(alexaRequest);
                    case "IntentRequest": return await OnIntentRequest(alexaRequest);
                    case "SessionEndedRequest": return await OnSessionEndRequest(alexaRequest);
                    case "LaunchRequest": return await OnLaunchRequest(alexaRequest);
                    case "System.ExceptionEncountered": return await OnExceptionEncountered();
                    default: return await OnDefault();
                }
            }
        }

        private static async Task<string> OnExceptionEncountered()
        {
            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                outputSpeech = new OutputSpeech() { phrase = "I have encountered an error." },
                shouldEndSession = true,
            }, null);
        }

        private async Task<string> OnIntentRequest(IAlexaRequest alexaRequest)
        {
            IAlexaSession session = null;

            var request = alexaRequest.request;
            var intent = request.intent;
            var context = alexaRequest.context;
            var system = context.System;
            var person = system.person;

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

                //There can not be a room intent request without any prior session context data.
                if (session.PersistedRequestData is null && IsRoomNameIntentRequest(intent))
                {
                    //end the session.
                    return await new NotUnderstood(alexaRequest, session).Response();
                }
            }

            try
            {
                /*
                * Amazon Alexa Custom SKill Console does not allow "." in skill names.
                * This would make creating namespace paths easier.
                * Instead we save the skill name with "_", which replaces the "." in the reflected path to the corresponding .cs file.
                * Replace the "_" (underscore) with a "." (period) to create the proper reflection path to the corresponding IntentRequest file.
                */
                var intentName = intent.name.Replace("_", ".");
                ServerController.Instance.Log.Info($"Intent Name Route to {intentName}");
                return await GetResponseResult(Type.GetType($"AlexaController.Api.IntentRequest.{intentName}"), alexaRequest, session);
            }
            catch (Exception exception)
            {
                var dataSource = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync(exception.Message, "/particles");
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = $"{Ssml.SayWithEmotion($"Sorry, I was unable to do that. {exception.Message}", Emotion.excited, Intensity.low)}",
                    },

                    directives = new List<IDirective>()
                    {
                        await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(dataSource, session)
                    }
                }, session);
            }
        }

        private static async Task<string> OnSessionEndRequest(IAlexaRequest alexaRequest)
        {
            await Task.Run(() => AlexaSessionManager.Instance.EndSession(alexaRequest));
            return null;
        }

        private static async Task<string> OnUserEvent(IAlexaRequest alexaRequest)
        {
            var request = alexaRequest.request;

            return await GetResponseResult(Type.GetType($"AlexaController.Api.UserEvent.{request.source.type}.{request.source.handler}.{request.arguments[0]}"), alexaRequest, null);
        }

        private static async Task<string> OnLaunchRequest(IAlexaRequest alexaRequest)
        {
            var context = alexaRequest.context;
            var user = SpeechAuthorization.Instance.GetRecognizedPersonalizationProfileResult(context.System.person);

            if (user is null)
            {
                var personNotRecognizedAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
                {
                    SpeechResponseType = SpeechResponseType.PersonNotRecognized
                });
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(
                    new Response()
                    {
                        shouldEndSession = true,
                        directives = new List<IDirective>()
                        {
                            await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(personNotRecognizedAudioProperties)
                        }
                    }, null);
            }

            var session = AlexaSessionManager.Instance.GetSession(alexaRequest, user);
            var genericLayoutProperties = await DataSourcePropertiesManager.Instance.GetGenericViewPropertiesAsync("Welcome to Home Theater Emby Controller", "/particles");

            var skillLaunchedAudioProperties = await DataSourcePropertiesManager.Instance.GetAudioResponsePropertiesAsync(new InternalAudioResponseQuery()
            {
                SpeechResponseType = SpeechResponseType.OnLaunch
            });

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                shouldEndSession = false,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.RenderVisualDocumentDirectiveAsync(genericLayoutProperties, session),
                    await RenderDocumentDirectiveManager.Instance.RenderAudioDocumentDirectiveAsync(skillLaunchedAudioProperties)
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




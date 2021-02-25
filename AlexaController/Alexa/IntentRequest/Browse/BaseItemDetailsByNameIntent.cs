using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Model.RequestData;
using AlexaController.Alexa.Model.ResponseData;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.Presentation.APLA.Filters;
using AlexaController.Alexa.Presentation.DirectiveBuilders;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BaseItemDetailsByNameIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public BaseItemDetailsByNameIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session = session;
        }
        public async Task<string> Response()
        {
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            }
            catch { }

           
            if (Session.room is null && Equals(Session.supportsApl, false)) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = slots.Movie.value is null ? slots.Series.value is null ? "" : "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;

            ServerController.Instance.Log.Info(searchName);
            
#pragma warning disable 4014
            Task.Run(() => ResponseClient.Instance.PostProgressiveResponse("One moment Please...", apiAccessToken, requestId)).ConfigureAwait(false);
#pragma warning restore 4014
            
            //Clean up search term
            searchName = StringNormalization.ValidateSpeechQueryString(searchName);

            if (string.IsNullOrEmpty(searchName)) return await new NotUnderstood(AlexaRequest, Session).Response();
            
            var result = ServerQuery.Instance.QuerySpeechResultItem(searchName, new[] { type });
            
            if (result is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderAudioManager.Instance.GetAudioDirectiveAsync(new InternalRenderAudioQuery()
                        {
                            speechContent = SpeechContent.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY,
                            session = Session,
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        })
                    }
                }, Session);
            }

            //User should not access this item. Warn the user, and place a notification in the Emby Activity Label
            if (!result.IsParentalAllowed(Session.User))
            {
                try
                {
                    if (Plugin.Instance.Configuration.EnableServerActivityLogNotifications)
                    {
                        await ServerController.Instance.CreateActivityEntry(LogSeverity.Warn,
                            $"{Session.User} attempted to view a restricted item.",
                            $"{Session.User} attempted to view {result.Name}.");
                    }
                }
                catch { }

                return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(
                            new InternalRenderDocumentQuery()
                            {
                                renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                                HeadlinePrimaryText = $"Stop! Rated {result.OfficialRating}"

                            }, Session),
                        await RenderAudioManager.Instance.GetAudioDirectiveAsync(
                            new InternalRenderAudioQuery()
                            {
                                speechContent = SpeechContent.PARENTAL_CONTROL_NOT_ALLOWED,
                                session = Session,
                                items = new List<BaseItem>() { result },
                                audio = new Audio()
                                {
                                    source = "soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02"
                                }
                            })
                    }
                }, Session);
            }
            

            if (!(Session.room is null))
            {
                try
                {
                    await ServerController.Instance.BrowseItemAsync(Session, result);
                }
                catch (Exception exception)
                {
#pragma warning disable 4014
                    Task.Run(() =>
                            ResponseClient.Instance.PostProgressiveResponse(exception.Message, apiAccessToken,
                                requestId))
                        .ConfigureAwait(false);
#pragma warning restore 4014
                    await Task.Delay(1200);
                    Session.room = null;
                }
            }

            var documentTemplateInfo = new InternalRenderDocumentQuery()
            {
                baseItems = new List<BaseItem>() { result },
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                HeaderAttributionImage = result.HasImage(ImageType.Logo) ? $"/Items/{result.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };
            
            var renderAudioTemplateInfo = new InternalRenderAudioQuery()
            {
                speechContent = SpeechContent.BROWSE_ITEM,
                session = Session,
                items = new List<BaseItem> { result },
                audio = new Audio()
                {
                    source = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };

            //Update Session
            Session.NowViewingBaseItem = result;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentManager.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);
            var renderAudioDirective    = await RenderAudioManager.Instance.GetAudioDirectiveAsync(renderAudioTemplateInfo);

            try
            {
                return await ResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = null,
                    SpeakUserName = true,
                    directives = new List<IDirective>()
                    {
                        renderDocumentDirective,
                        renderAudioDirective
                    }

                }, Session);

            }
            catch (Exception exception)
            {
                throw new Exception("I was unable to build the render document. " + exception.Message);
            }
        }
    }
}
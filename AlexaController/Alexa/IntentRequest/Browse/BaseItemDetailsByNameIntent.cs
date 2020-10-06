using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.LexicalSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;

namespace AlexaController.Alexa.IntentRequest.Browse
{
    [Intent]
    public class BaseItemDetailsByNameIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session      { get; }

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

            var displayNone = Equals(Session.alexaSessionDisplayType, AlexaSessionDisplayType.NONE);
            if (Session.room is null && displayNone) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            

            var request        = AlexaRequest.request;
            var intent         = request.intent;
            var slots          = intent.slots;
            var type           = slots.Movie.value is null ? "Series" : "Movie";
            var searchName     = (slots.Movie.value ?? slots.Series.value) ?? slots.@object.value;
            var context        = AlexaRequest.context;
            var apiAccessToken = context.System.apiAccessToken;
            var requestId      = request.requestId;


            var progressiveSpeech = await SpeechStrings.GetPhrase(new SpeechStringQuery()
            {
                type = SpeechResponseType.PROGRESSIVE_RESPONSE, 
                session = Session
            });

#pragma warning disable 4014
            ResponseClient.Instance.PostProgressiveResponse(progressiveSpeech, apiAccessToken, requestId).ConfigureAwait(false);
#pragma warning restore 4014

            //Clean up search term
            searchName = StringNormalization.ValidateSpeechQueryString(searchName);

            if (string.IsNullOrEmpty(searchName)) return await new NotUnderstood(AlexaRequest, Session).Response(); 

            var result = EmbyServerEntryPoint.Instance.QuerySpeechResultItem(searchName, new[] { type }, Session.User);

            if (result is null)
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                        {
                            type = SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, 
                            session = Session
                        }),
                    }
                });
            }

            if (!result.IsParentalAllowed(Session.User))
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    shouldEndSession = true,
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                        {
                            type = SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED,
                            session = Session,
                            items = new List<BaseItem>() {result}
                        }),
                        sound = "<audio src=\"soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02\"/>"
                    },
                    directives = new List<IDirective>()
                    {
                        await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(
                            new RenderDocumentTemplate()
                            {
                                renderDocumentType = RenderDocumentType.GENERIC_HEADLINE_TEMPLATE,
                                HeadlinePrimaryText = $"Stop! Rated {result.OfficialRating}"
                            }, Session)
                    }
                });
            }

            if (!(Session.room is null))
            {
                try
                {
                    await EmbyServerEntryPoint.Instance.BrowseItemAsync(Session, result);
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

            var documentTemplateInfo = new RenderDocumentTemplate()
            {
                baseItems = new List<BaseItem>() { result },
                renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE,
                HeaderAttributionImage = result.HasImage(ImageType.Logo) ? $"/Items/{result.Id}/Images/logo?quality=90&amp;maxHeight=708&amp;maxWidth=400&amp;" : null
            };

            //Update Session
            Session.NowViewingBaseItem = result;
            AlexaSessionManager.Instance.UpdateSession(Session, documentTemplateInfo);

            var renderDocumentDirective = await RenderDocumentBuilder.Instance.GetRenderDocumentDirectiveAsync(documentTemplateInfo, Session);

            try
            {
                return await ResponseClient.Instance.BuildAlexaResponse(new Response()
                {
                    outputSpeech = new OutputSpeech()
                    {
                        phrase = await SpeechStrings.GetPhrase(new SpeechStringQuery()
                        {
                            type = SpeechResponseType.BROWSE_ITEM, 
                            session = Session, 
                            items = new List<BaseItem> { result }
                        }),
                        sound = "<audio src=\"soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13\"/>"
                    },
                    shouldEndSession = null,
                    directives = new List<IDirective>()
                    {
                        renderDocumentDirective
                    }

                }, Session.alexaSessionDisplayType);

            }
            catch (Exception exception)
            {
                throw new Exception("I was unable to build the render document. " + exception.Message);
            }
        }
    }
}
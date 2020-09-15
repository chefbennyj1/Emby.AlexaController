using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Api;
using AlexaController.Session;
using AlexaController.Utils;
using AlexaController.Utils.SemanticSpeech;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments

namespace AlexaController.Alexa.Presentation.APL.UserEvent.TouchWrapper.Press
{
    public class UserEventPlaybackStart : UserEventResponse
    {
        public override string Response
        (AlexaRequest alexaRequest, ILibraryManager libraryManager, IResponseClient responseClient, ISessionManager sessionManager)
        {
            var source = alexaRequest.request.source;
            var session = AlexaSessionManager.Instance.GetSession(alexaRequest);
            var baseItem = libraryManager.GetItemById(source.id);
            var room =  session.room;
            
            var responseData = new Response();
            
            if (room is null)
            {
                session.NowViewingBaseItem = baseItem;
                AlexaSessionManager.Instance.UpdateSession(session);

                responseData.shouldEndSession = null;
                responseData.directives = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                    {
                        renderDocumentType = RenderDocumentType.ROOM_SELECTION_TEMPLATE,
                        baseItems          = new List<BaseItem>() { baseItem }

                    }, session)
                };

                return responseClient.BuildAlexaResponse(responseData, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);
            }

            session.PlaybackStarted = true;
            AlexaSessionManager.Instance.UpdateSession(session);

            Task.Run(() => EmbyControllerUtility.Instance.PlayMediaItemAsync(session, baseItem, session.User));

            return responseClient.BuildAlexaResponse(new Response()
            {
                person = session.person,
                outputSpeech = new OutputSpeech()
                {
                    phrase         = SemanticSpeechStrings.GetPhrase(SpeechResponseType.PLAY_MEDIA_ITEM, session, new List<BaseItem>() {baseItem}),
                    semanticSpeechType = SemanticSpeechType.COMPLIANCE
                },
                shouldEndSession = null,
                directives = new List<Directive>()
                {
                    RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
                    {
                        baseItems          = new List<BaseItem>() {baseItem},
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, session)
                }
            }, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);   

            

            //return responseClient.BuildAlexaResponse(new Response()
            //{
            //    person = session.person,
            //    outputSpeech = new OutputSpeech()
            //    {
            //        phrase = SpeechResponseStrings.GetPhrase(ResponseType.PLAY_MEDIA_ITEM, room, new List<BaseItem>() { baseItem }, session.person),
            //        semanticSpeech = SpeechUtility.SemanticSpeech.Compliance,
            //        emotion = Emotion.excited,
            //        intensity = Intensity.low
            //    },
            //    shouldEndSession = null,
            //    directives = new List<Directive>()
            //    {
            //        RenderDocumentBuilder.Instance.GetRenderDocumentTemplate(new RenderDocumentTemplateInfo()
            //        {
            //            baseItems          = new List<BaseItem>() { baseItem },
            //            displayBackButton  = false,
            //            HeaderTitle        = $"Now Playing {baseItem.Name}",
            //            renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

            //        }, session)
            //    }

            //}, AlexaSessionDisplayType.ALEXA_PRESENTATION_LANGUAGE);

        }
    }
}

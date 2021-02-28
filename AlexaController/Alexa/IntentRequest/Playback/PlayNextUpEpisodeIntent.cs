﻿using System.Collections.Generic;
using System.Threading.Tasks;
using AlexaController.Alexa.IntentRequest.Rooms;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Api;
using AlexaController.Api.RequestData;
using AlexaController.Api.ResponseModel;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;


namespace AlexaController.Alexa.IntentRequest.Playback
{
    [Intent]
    public class PlayNextUpEpisodeIntent : IIntentResponse
    {
        public IAlexaRequest AlexaRequest { get; }
        public IAlexaSession Session { get; }
        
        public PlayNextUpEpisodeIntent(IAlexaRequest alexaRequest, IAlexaSession session)
        {
            AlexaRequest = alexaRequest;
            Session      = session;
        }
        public async Task<string> Response()
        {
            //we need a room object
            try
            {
                Session.room = RoomManager.Instance.ValidateRoom(AlexaRequest, Session);
            } 
            catch { }
            if (Session.room is null) return await RoomManager.Instance.RequestRoom(AlexaRequest, Session);
            
            var request       = AlexaRequest.request;
            var intent        = request.intent;
            var slots         = intent.slots;
            var nextUpEpisode = ServerQuery.Instance.GetNextUpEpisode(slots.Series.value, Session?.User);

            if (nextUpEpisode is null)
            {
                return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
                {
                    shouldEndSession = true,
                    directives = new List<IDirective>()
                    {
                        await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(new AudioDirectiveQuery()
                        {
                            speechContent = SpeechContent.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY,
                            session = Session,
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        })
                    }
                    //outputSpeech = new OutputSpeech()
                    //{
                    //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                    //    {
                    //        type = SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY, 
                    //        session = Session
                    //    })
                    //},
                }, Session);
            }

#pragma warning disable 4014
            Task.Run(() => ServerController.Instance.PlayMediaItemAsync(Session, nextUpEpisode)).ConfigureAwait(false);
#pragma warning restore 4014

           
            Session.NowViewingBaseItem = nextUpEpisode;
            AlexaSessionManager.Instance.UpdateSession(Session, null);

            return await AlexaResponseClient.Instance.BuildAlexaResponseAsync(new Response()
            {
                //outputSpeech = new OutputSpeech()
                //{
                //    phrase = await SpeechStrings.GetPhrase(new RenderAudioTemplate()
                //    {
                //        type = SpeechResponseType.PLAY_NEXT_UP_EPISODE, 
                //        session = Session, 
                //        items = new List<BaseItem>() { nextUpEpisode }
                //    }),
                   
                //},
                shouldEndSession = true,
                directives = new List<IDirective>()
                {
                    await RenderDocumentDirectiveManager.Instance.GetRenderDocumentDirectiveAsync(new RenderDocumentQuery()
                    {
                        baseItems          = new List<BaseItem>() { nextUpEpisode },
                        renderDocumentType = RenderDocumentType.ITEM_DETAILS_TEMPLATE

                    }, Session),
                    await AudioDirectiveManager.Instance.GetAudioDirectiveAsync(
                        new AudioDirectiveQuery()
                        {
                            speechContent = SpeechContent.PLAY_NEXT_UP_EPISODE, 
                            session = Session, 
                            items = new List<BaseItem>() { nextUpEpisode },
                            audio = new Audio()
                            {
                                source ="soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                                
                            }
                        })
                }
            }, Session);
        }
    }
}

using System.Collections.Generic;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

namespace AlexaController.Alexa.Presentation.DirectiveBuilders
{
    public enum SpeechPrefix
    {
        REPOSE,
        APOLOGETIC,
        COMPLIANCE,
        NONE,
        GREETINGS,
        NON_COMPLIANT,
        DEFAULT
    }

    public enum SpeechContent
    {
        PARENTAL_CONTROL_NOT_ALLOWED,
        BROWSE_NEXT_UP_EPISODE,
        NO_NEXT_UP_EPISODE_AVAILABLE,
        NO_SEASON_ITEM_EXIST,
        BROWSE_ITEM,
        BROWSE_LIBRARY,
        BROWSE_ITEMS_BY_ACTOR,
        DISPLAY_MOVIE_COLLECTION,
        NEW_ITEMS_APL,
        NEW_ITEMS_DISPLAY_NONE,
        PLAY_MEDIA_ITEM,
        PLAY_NEXT_UP_EPISODE,
        ROOM_CONTEXT,
        GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY,
        NO_DEVICE_CONFIGURATION,
        DEVICE_UNAVAILABLE,
        VOICE_AUTHENTICATION_ACCOUNT_EXISTS,
        VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR,
        VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS,
        PERSON_NOT_RECOGNIZED,
        NOT_UNDERSTOOD,
        ON_LAUNCH,
        PROGRESSIVE_RESPONSE,
        UP_COMING_EPISODES
    }

    public class InternalRenderAudioQuery
    {
        public SpeechContent speechContent { get; set; }
        public SpeechPrefix speechPrefix   { get; set; } = SpeechPrefix.DEFAULT;
        public IAlexaSession session       { get; set; }
        public List<BaseItem> items        { get; set; }
        public Audio audio                 { get; set; } 
        public string[] args               { get; set; }
    }
}

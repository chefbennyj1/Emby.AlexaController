using System;
using System.Collections.Generic;
using System.Linq;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;

// ReSharper disable InconsistentNaming
// ReSharper disable TooManyArguments
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable twice AssignNullToNotNullAttribute

namespace AlexaController.Utils.SemanticSpeech
{
    public enum SpeechResponseType
    {
        PARENTAL_CONTROL_NOT_ALLOWED,
        BROWSE_NEXT_UP_EPISODE,
        NO_NEXT_UP_EPISODE_AVAILABLE,
        NO_SEASON_ITEM_EXIST,
        BROWSE_ITEM,
        BROWSE_LIBRARY,
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
        VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS
    }

    public class SemanticSpeechStrings : SemanticSpeechUtility
    {

        public static readonly List<string> HelpStrings = new List<string>()
        {
            "Welcome to help! Let's get started. Swipe left to continue...",
            "<b>Accessing Emby accounts based on your voice.</b>",
            "I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now.",
            "Next, open the plugin configuration in emby server. Select the Personalization button, and follow the instructions to add personalization.",
            "<b>Accessing media Items in rooms</b>",
            "The Emby plugin will allow you to create rooms, based on devices in your home.",
            "In the plugin configuration, map each Emby ready device to a specific room. You create the room name that I will understand.",
            "For example: map your Amazon Fire Stick 4K to a room named: \"Family Room\".",
            "Now you can access titles and request them to display per room.",
            "You can use the phrase:",
            "Ask home theater to play the movie Iron Man in the family room.",
            "To display the movies library on the \"Family Room\" device, you can use the phrase:",
            "Ask home theater to show \"Movies\" in the Family Room.",
            "The same can be said for \"Collections\", and \"TV Series\" libraries",
            "The Emby client must already be running on the device in order to access the room commands.",
            "<b>Accessing Collection Items</b>",
            "I have the ability to show collection data on echo show, echo spot, or other devices with screens.",
            "To access this ability, you can use the following phrases:",
            "Ask home theater to show all the Iron Man movies...",
            "Ask home theater to show the Spiderman collection...",
            "Fire TV devices, with Alexa enabled, are exempt from displaying these items because the Emby client will take care of this for you.",
            "<b>Accessing individual media Items</b>",
            "I am able to show individual media items as well.",
            "You can use the phrases:",
            "Ask home theater to show the movie Spiderman Far From Home...",
            "Ask home theater to show the movie Spiderman Far From Home, in the Family Room.",
            "You can also access TV Series the same way.",
            "I will understand the phrases: ",
            "Ask home theater to show the series Westworld...",
            "Ask home theater to show the next up episode for Westworld...",
            "<b>Accessing new media Items</b>",
            "To access new titles in your library, you can use the phrases:",
            "Ask home theater about new movies...",
            "Ask home theater about new TV Series...",
            "You can also request a duration for new items... For example:",
            "Ask home theater for new movies added in the last three days...",
            "Ask home theater for new tv shows added in the last month",
            "Remember that as long as the echo show, or spot is displaying an image, you are in an open session. This means you don't have to use the \"Ask home theater\" phrases to access media",
            "This concludes the help section. Good luck!",
        };

        public static string GetPhrase
        (SpeechResponseType type, IAlexaSession session, List<BaseItem> items = null, string[] args = null)
        {

            switch (type)
            {
                case SpeechResponseType.NO_SEASON_ITEM_EXIST:

                    return
                        $"{SpeechRate(Rate.fast, SayWithEmotion(StringNormalization.ValidateSpeechQueryString(session?.NowViewingBaseItem.Name), Emotion.disappointed, Intensity.high))} ... " +
                        $"{ExpressiveInterjection("doesn't contain")} season {args?[0]}!";

                case SpeechResponseType.BROWSE_ITEM:

                    return $"{StringNormalization.ValidateSpeechQueryString(items?[0].Name)}, Rated {items?[0].OfficialRating}. {(session.room != null ? $" Showing in the {session.room.Name}." : string.Empty)}";
                           

                case SpeechResponseType.BROWSE_NEXT_UP_EPISODE:
                    
                    return $"Here is the next up episode for {items?[0].Parent.Parent.Name}. " +
                           $"{items?[0].Name}." +
                           (session.room != null ? $" Showing in the {session.room}." : string.Empty);
                
                case SpeechResponseType.DISPLAY_MOVIE_COLLECTION:

                    return $"The {items?[0].Name}";

                case SpeechResponseType.BROWSE_LIBRARY:

                    return $"Here is the {items?[0].Name} Library.";

                case SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED:

                    return GetSemanticSpeechResponse(SemanticSpeechType.NON_COMPLIANT) + " " + SayInDomain(Domain.news,
                               $"Are you sure you are allowed access to {(!(items is null) ? items[0].Name : " this item ")}, " +
                               $"{(!(session.person is null) ? SayName(session.person) : "")}?");
                
                case SpeechResponseType.PLAY_MEDIA_ITEM:

                    return $"Now playing the {items?[0].ProductionYear} {items?[0].GetType().Name} " +
                           $"{InsertStrengthBreak(StrengthBreak.weak)} {items?[0].Name}.";

                case SpeechResponseType.ROOM_CONTEXT:

                    return SayInDomain(Domain.conversational, "I didn't get the room you wish to view that. Which room did you want?");
                    
                case SpeechResponseType.PLAY_NEXT_UP_EPISODE:

                    return $"Playing the next up episode for {items?[0].Parent.Parent.Name}. Showing in the {session.room}";

                case SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY:

                    return $"{SayWithEmotion("That item ", Emotion.disappointed, Intensity.high)} ... {ExpressiveInterjection("doesn't exist")} in the library!";

                case SpeechResponseType.NO_DEVICE_CONFIGURATION:

                    return $"There is no device configuration for {session.room}. " +
                           "Please look in the plugin configuration to map rooms to emby ready devices.";

                case SpeechResponseType.DEVICE_UNAVAILABLE:

                    return $"{SayWithEmotion("I was currently unable to access that device.", Emotion.disappointed,Intensity.high)}" + 
                           InsertStrengthBreak(StrengthBreak.weak) +
                           "Please make sure it is available on the network, so I can stream to it";

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS:

                    return $"This profile is already linked to { SayName(session.person) }'s account";

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR:

                    return "I was unable to learn your voice. Please make sure you have allowed personalization in the Alexa app, " +
                           " <break strength='weak' /> and try again.";

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS:

                    return $"Success { SayName(session.person) }! " +
                           "Please look at the plugin configuration. " +
                           $"You should now see the I.D. linked to your voice. {InsertStrengthBreak(StrengthBreak.weak)}" +
                           " Choose your emby account name and press save.";

                case SpeechResponseType.NEW_ITEMS_DISPLAY_NONE:

                    var s = string.Empty;
                   
                    s = $"There {(items?.Count > 1 ? "are" : "is")} " +
                        $"{SayAsCardinal(items?.Count.ToString())} new " +
                        $"{(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name)}. ";
                   
                    s += string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}", items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name)));

                    return s;

                case SpeechResponseType.NEW_ITEMS_APL:

                    return $"There {(items?.Count > 1 ? "are" : "is")} " +
                           $"{SayAsCardinal(items?.Count.ToString())} new " +
                           $"{(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name)}";

                case SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE:

                    return "There doesn't seem to be a new episode available for that series.";

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            
        }
    }
}

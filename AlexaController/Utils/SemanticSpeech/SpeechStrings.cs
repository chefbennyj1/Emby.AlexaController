using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS,
        NOT_UNDERSTOOD,
        ON_LAUNCH,
        PROGRESSIVE_RESPONSE
    }

    public class SpeechStrings : Semantics
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
        
        public static string GetPhrase(SpeechResponseType type, IAlexaSession session, List<BaseItem> items = null, string[] args = null)
        {
            var speech = new StringBuilder();
            var name = string.Empty;
            switch (type)
            {
                case SpeechResponseType.PROGRESSIVE_RESPONSE:
                    speech.Append(GetRandomSpeechResponse(SpeechType.COMPLIANCE));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(GetRandomSpeechResponse(SpeechType.REPOSE));
                    return speech.ToString();

                case SpeechResponseType.ON_LAUNCH:
                    speech.Append(GetRandomSpeechResponse(SpeechType.GREETINGS));
                    if (session.person != null)
                    {
                        speech.Append(SayName(session.person));
                    }  
                    speech.Append(InsertStrengthBreak(StrengthBreak.strong));
                    speech.Append("What media can I help you find.");
                    return speech.ToString();

                case SpeechResponseType.NOT_UNDERSTOOD:
                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("I misunderstood what you said.");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
                    return speech.ToString();

                case SpeechResponseType.NO_SEASON_ITEM_EXIST:
                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    name = StringNormalization.ValidateSpeechQueryString(session?.NowViewingBaseItem.Name);
                    speech.Append(SpeechRate(Rate.fast, SayWithEmotion(name, Emotion.disappointed, Intensity.high)));
                    speech.Append(ExpressiveInterjection(" doesn't contain "));
                    speech.Append(" season ");
                    speech.Append(args?[0]);
                    return speech.ToString();

                case SpeechResponseType.BROWSE_ITEM:
                    name = StringNormalization.ValidateSpeechQueryString(items?[0].Name);
                    speech.Append(name);
                    speech.Append(", ");
                    speech.Append("Rated ");
                    speech.Append(items?[0].OfficialRating);
                    if (session.room is null)
                    {
                        return speech.ToString();
                    }
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(session.room.Name);
                    return speech.ToString();  

                case SpeechResponseType.BROWSE_NEXT_UP_EPISODE:

                    var season = items?[0].Parent;
                    var seriesName = season?.Parent.Name;
                    speech.Append("Here is the next up episode for ");
                    speech.Append(seriesName);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(items?[0].Name);
                    if (session.room is null)
                    {
                        return speech.ToString();
                    }
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(session.room.Name);
                    return speech.ToString();
                   
                case SpeechResponseType.DISPLAY_MOVIE_COLLECTION:

                    speech.Append("The ");
                    speech.Append(items?[0].Name);
                    return speech.ToString();

                case SpeechResponseType.BROWSE_LIBRARY:

                    speech.Append("Here is the ");
                    speech.Append(items?[0].Name);
                    speech.Append(" library.");
                    return speech.ToString();

                case SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED:
                   
                    speech.Append(GetRandomSpeechResponse(SpeechType.NON_COMPLIANT));
                    speech.Append("Are you sure you are allowed access to ");
                    speech.Append(items is null ? "this item" : items[0].Name);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayName(session.person));
                    speech.Append("?");
                    return speech.ToString();
                
                case SpeechResponseType.PLAY_MEDIA_ITEM:

                    speech.Append("Now playing the ");
                    speech.Append(items?[0].ProductionYear);
                    speech.Append(" ");
                    speech.Append(items?[0].GetType().Name);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak)); 
                    speech.Append(items?[0].Name);
                    return speech.ToString();

                case SpeechResponseType.ROOM_CONTEXT:

                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SayInDomain(Domain.conversational, "I didn't get the room you wish to view that."));
                    speech.Append(SayInDomain(Domain.conversational, "Which room did you want?"));
                    return speech.ToString();
                    
                case SpeechResponseType.PLAY_NEXT_UP_EPISODE:

                    speech.Append("Playing the next up episode for ");
                    speech.Append(items?[0].Parent.Parent.Name);
                    speech.Append("Showing in the ");
                    speech.Append(session.room.Name);
                    return speech.ToString();

                case SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY:

                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SayWithEmotion("That item ", Emotion.disappointed, Intensity.high));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(ExpressiveInterjection("doesn't exist "));
                    speech.Append("in the library!");
                    return speech.ToString();

                case SpeechResponseType.NO_DEVICE_CONFIGURATION:

                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("There is no device configuration for ");
                    speech.Append(session.room);
                    speech.Append("Please look in the plugin configuration to map rooms to emby ready devices.");
                    return speech.ToString();

                case SpeechResponseType.DEVICE_UNAVAILABLE:

                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SayWithEmotion("I was unable to access ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SayWithEmotion("the device in the ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SayWithEmotion(args?[0], Emotion.disappointed, Intensity.medium));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    return speech.ToString();

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS:

                    speech.Append("This profile is already linked to ");
                    speech.Append(SayName(session.person));
                    speech.Append("'s account");
                    return speech.ToString();

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR:

                    speech.Append("I was unable to learn your voice.");
                    speech.Append("Please make sure you have allowed personalization in the Alexa app.");
                    return speech.ToString();

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS:

                    speech.Append("Success ");
                    speech.Append(SayName(session.person));
                    speech.Append("!");
                    speech.Append("Please look at the plugin configuration.");
                    speech.Append("You should now see the I.D. linked to your voice.");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Choose your emby account name and press save.");
                    return speech.ToString();

                case SpeechResponseType.NEW_ITEMS_DISPLAY_NONE:
                    
                    speech.Append("There ");
                    speech.Append(items?.Count > 1 ? "are" : "is");
                    speech.Append(SayAsCardinal(items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);
                    speech.Append(string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}",
                        items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
                    return speech.ToString();

                case SpeechResponseType.NEW_ITEMS_APL:

                    speech.Append("There ");
                    speech.Append(items?.Count > 1 ? "are" : "is");
                    speech.Append(SayAsCardinal(items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);
                    return speech.ToString();

                case SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE:

                    speech.Append(GetRandomSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("There doesn't seem to be a new episode available for that series.");
                    return speech.ToString();

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}

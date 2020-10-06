﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.Speech;
using AlexaController.Session;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Extensions;

namespace AlexaController.Utils.LexicalSpeech
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
        PERSON_NOT_RECOGNIZED,
        NOT_UNDERSTOOD,
        ON_LAUNCH,
        PROGRESSIVE_RESPONSE,
        UP_COMING_EPISODES
    }

    public class SpeechStringQuery
    {
        public SpeechResponseType type { get; set; }
        public IAlexaSession session   { get; set; }
        public List<BaseItem> items    { get; set; }
        public string[] args           { get; set; }
    }

    public class SpeechStrings : Lexicons
    {
        public static readonly List<string> HelpStrings = new List<string>()
        {
            "Welcome to help! Let's get started. Swipe left to continue...",
            "<b>Accessing Emby accounts based on your voice.</b>",
            "I have the ability to access specific emby user accounts, and library data based on the sound of your voice. If you have not yet turned on Personalization in your Alexa App, and enabled it for this skill, please do that now.",
            "You can enable Parental Controls, so media will be filtered based on who is speaking. This way, media items can not be accessed by people who shouldn't have access to them.",
            "To enable this feature, toggle the button for \"Enable parental control using voice recognition\". If this feature is turned off, I will not filter media items, and will show media based on the Emby Administrators account, at all times.",
            "Next, open the plugin configuration in emby server. Select the \"New Authorization Account Link\" button, and follow the instructions to add personalization.",
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

        // ReSharper disable RedundantTypeArgumentsOfMethod
        // ReSharper disable TooManyArguments

        public static async Task<string> GetPhrase(SpeechStringQuery speechQuery)
        {
            var speech = new StringBuilder();
            var name   = string.Empty;
            switch (speechQuery.type)
            {
                case SpeechResponseType.PERSON_NOT_RECOGNIZED:
                {
                    speech.Append("I don't recognize the current user.");
                    speech.Append("Please go to the plugin configuration and link emby account personalization.");
                    speech.Append("Or ask for help.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.PROGRESSIVE_RESPONSE:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.COMPLIANCE));
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.REPOSE));
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.ON_LAUNCH:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.GREETINGS));
                    if (speechQuery.session.person != null)
                    {
                        speech.Append(SayName(speechQuery.session.person));
                    }  
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.strong));
                    speech.Append("What media can I help you find.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.NOT_UNDERSTOOD:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("I misunderstood what you said.");
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SpeechStyle.SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.NO_SEASON_ITEM_EXIST:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    name = StringNormalization.ValidateSpeechQueryString(speechQuery.session?.NowViewingBaseItem.Name);
                    speech.Append(SpeechStyle.SpeechRate(Rate.fast, SpeechStyle.SayWithEmotion(name, Emotion.disappointed, Intensity.high)));
                    speech.Append(SpeechStyle.ExpressiveInterjection(" doesn't contain "));
                    speech.Append(" season ");
                    speech.Append(speechQuery.args?[0]);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.BROWSE_ITEM:
                {
                    name = StringNormalization.ValidateSpeechQueryString(speechQuery.items?[0].Name);
                    var rating = speechQuery.items?[0].OfficialRating;
                    speech.Append(name);
                    speech.Append(", ");
                    if (string.IsNullOrEmpty(rating))
                    {
                        speech.Append("unrated");
                    }
                    else
                    {
                        speech.Append("Rated ");
                        speech.Append(rating);
                    }

                    if (speechQuery.session.room is null)
                    {
                        return speech.ToString();
                    }
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(speechQuery.session.room.Name);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.BROWSE_NEXT_UP_EPISODE:
                {
                    var season = speechQuery.items?[0].Parent;
                    var seriesName = season?.Parent.Name;
                    speech.Append("Here is the next up episode for ");
                    speech.Append(seriesName);
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(speechQuery.items?[0].Name);
                    if (speechQuery.session.room is null)
                    {
                        return await Task.FromResult<string>(speech.ToString());
                    }
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(speechQuery.session.room.Name);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.DISPLAY_MOVIE_COLLECTION:
                {
                    speech.Append("The ");
                    speech.Append(speechQuery.items?[0].Name);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.BROWSE_LIBRARY:
                {
                    speech.Append("Here is the ");
                    speech.Append(speechQuery.items?[0].Name);
                    speech.Append(" library.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.PARENTAL_CONTROL_NOT_ALLOWED:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.NON_COMPLIANT));
                    speech.Append("Are you sure you are allowed access to ");
                    speech.Append(speechQuery.items is null ? "this item" : speechQuery.items[0].Name);
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayName(speechQuery.session.person));
                    speech.Append("?");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.PLAY_MEDIA_ITEM:
                {
                    speech.Append("Now playing the ");
                    speech.Append(speechQuery.items?[0].ProductionYear);
                    speech.Append(" ");
                    speech.Append(speechQuery.items?[0].GetType().Name);
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak)); 
                    speech.Append(speechQuery.items?[0].Name);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.ROOM_CONTEXT:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SpeechStyle.SayInDomain(Domain.conversational, "I didn't get the room you wish to view that."));
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SpeechStyle.SayInDomain(Domain.music, "Which room did you want?"));
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.PLAY_NEXT_UP_EPISODE:
                {
                    speech.Append("Playing the next up episode for ");
                    speech.Append(speechQuery.items?[0].Parent.Parent.Name);
                    speech.Append("Showing in the ");
                    speech.Append(speechQuery.session.room.Name);
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SpeechStyle.SayWithEmotion("That item ", Emotion.disappointed, Intensity.high));
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SpeechStyle.ExpressiveInterjection("doesn't exist "));
                    speech.Append("in the library!");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.NO_DEVICE_CONFIGURATION:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("There is no device configuration for ");
                    speech.Append(speechQuery.session.room);
                    speech.Append("Please look in the plugin configuration to map rooms to emby ready devices.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.DEVICE_UNAVAILABLE:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append(SpeechStyle.SayWithEmotion("I was unable to access ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SpeechStyle.SayWithEmotion("the device in the ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SpeechStyle.SayWithEmotion(speechQuery.args?[0], Emotion.disappointed, Intensity.medium));
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_EXISTS:
                {
                    speech.Append(GetSpeechDysfluency(Emotion.excited, Intensity.low, Rate.slow));
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.strong));
                    speech.Append("This profile is already linked to ");
                    speech.Append(SayName(speechQuery.session.person));
                    speech.Append("'s account");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR:
                {
                    speech.Append("I was unable to learn your voice.");
                    speech.Append("Please make sure you have allowed personalization in the Alexa app.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS:
                {
                    speech.Append(SpeechStyle.ExpressiveInterjection("Success "));
                    speech.Append(SpeechStyle.ExpressiveInterjection(SayName(speechQuery.session.person)));
                    speech.Append("!");
                    speech.Append("Please look at the plugin configuration.");
                    speech.Append("You should now see the I.D. linked to your voice.");
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Choose your emby account name and press save.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.UP_COMING_EPISODES:
                {
                    speech.Append("There ");
                    speech.Append(speechQuery.items?.Count > 1 ? "are" : "is");
                    speech.Append(SpeechStyle.SayAsCardinal(speechQuery.items?.Count.ToString()));
                    speech.Append(" upcoming episode");
                    speech.Append(speechQuery.items?.Count > 1 ? "s" : "");
                    
                    var date = DateTime.Parse(speechQuery.args[0]);
                    
                    speech.Append($" scheduled to air over the next {(date - DateTime.Now).Days} days.");
                    speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                   
                    var schedule = speechQuery.items.DistinctBy(item => item.Parent.ParentId);
                   
                    foreach (var item in schedule)
                    {
                        speech.Append(StringNormalization.ValidateSpeechQueryString(item.Parent.Parent.Name));
                        if (item.IndexNumber == 1)
                        {
                            speech.Append($" will premiere season {SpeechStyle.SayAsCardinal(item.Parent.IndexNumber.ToString())} ");
                            speech.Append(SpeechStyle.SayAsDate(Date.md, item.PremiereDate.Value.ToString("M/d"))); //("MMMM dd", CultureInfo.CreateSpecificCulture("en-US")))); 
                            speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                            speech.Append(" and ");
                        }
                        speech.Append(" will air on ");
                        speech.Append($"{item.PremiereDate.Value.DayOfWeek}'s");
                            
                        speech.Append(SpeechStyle.InsertStrengthBreak(StrengthBreak.strong));
                    }
                    
                    speech.Append(
                        $"This completes the list of episodes scheduled for the next {(date - DateTime.Now).Days} days. ");

                    return await Task.FromResult<string>(SpeechStyle.SayInDomain(Domain.conversational, speech.ToString()));
                }

                case SpeechResponseType.NEW_ITEMS_DISPLAY_NONE:
                {
                    speech.Append("There ");
                    speech.Append(speechQuery.items?.Count > 1 ? "are" : "is");
                    speech.Append(SpeechStyle.SayAsCardinal(speechQuery.items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(speechQuery.items?.Count > 1 ? speechQuery.items[0].GetType().Name + "s" : speechQuery.items?[0].GetType().Name);

                    var date = DateTime.Parse(speechQuery.args[0]);
                    speech.Append($" added in the past {(date - DateTime.Now).Days} days. ");

                    speech.Append(string.Join($", {SpeechStyle.InsertStrengthBreak(StrengthBreak.weak)}",
                        // ReSharper disable once AssignNullToNotNullAttribute
                        speechQuery.items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.NEW_ITEMS_APL:
                {
                    speech.Append("There ");
                    speech.Append(speechQuery.items?.Count > 1 ? "are" : "is");
                    speech.Append(SpeechStyle.SayAsCardinal(speechQuery.items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(speechQuery.items?.Count > 1 ? speechQuery.items[0].GetType().Name + "s" : speechQuery.items?[0].GetType().Name);

                    var date = DateTime.Parse(speechQuery.args[0]);
                    speech.Append($" added in the past {(date - DateTime.Now).Days *-1} days.");

                    return await Task.FromResult<string>(speech.ToString());
                }

                case SpeechResponseType.NO_NEXT_UP_EPISODE_AVAILABLE:
                {
                    speech.Append(GetRandomSemanticSpeechResponse(SpeechType.APOLOGETIC));
                    speech.Append("There doesn't seem to be a new episode available for that series.");
                    return await Task.FromResult<string>(speech.ToString());
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), speechQuery.type, null);
            }
        }
    }
}
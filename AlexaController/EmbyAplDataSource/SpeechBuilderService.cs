using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace AlexaController.EmbyAplDataSource
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum SpeechResponseType
    {
        PersonNotRecognized,
        OnLaunch,
        NotUnderstood,
        NoItemExists,
        ItemBrowse,
        BrowseNextUpEpisode,
        NoNextUpEpisodeAvailable,
        PlayNextUpEpisode,
        ParentalControlNotAllowed,
        PlayItem,
        RoomContext,
        VoiceAuthenticationExists,
        VoiceAuthenticationAccountLinkError,
        VoiceAuthenticationAccountLinkSuccess,
        UpComingEpisodes,
        NewLibraryItems,
        BrowseItemByActor
    }

    public abstract class SpeechBuilderService : Ssml
    {
        private static readonly List<string> Apologetic = new List<string>()
        {
            "Sorry",
            "Apologies.",
            "I apologize.",
            "I'm Sorry.",
            ""
        };

        private static readonly List<string> Compliance = new List<string>()
        {
            $"{ExpressiveInterjection("you bet!")}",
            $"{ExpressiveInterjection("OK")}",
            $"{ExpressiveInterjection("absolutely!")}",
            $"{ExpressiveInterjection("all righty!")}",
            $"{ExpressiveInterjection("as you wish!")}",
            "Alright, ",
            SayWithEmotion("Yes.", Emotion.excited, Intensity.medium),
            "Here you go."
        };

        private static readonly List<string> Repose = new List<string>()
        {
            "One moment...",
            "One moment please...",
            "Just a moment..."
        };

        private static readonly List<string> Greetings = new List<string>()
        {
            "Hey",
            "Hi",
            "Hello"
        };

        private static readonly List<string> DysfluencyNegative = new List<string>()
        {
            $"{ExpressiveInterjection("oops!")}",
            $"{ExpressiveInterjection("ugh!")}",
            $"{ExpressiveInterjection("jeez!")}",
            $"{ExpressiveInterjection("my goodness!")}",
            $"{ExpressiveInterjection("good grief!")}",
            $"{ExpressiveInterjection("darn!")}",
            $"{ExpressiveInterjection("alas!")}",
            $"{ExpressiveInterjection("uh ho!")}",
            $"{ExpressiveInterjection("um!")}",
            $"{ExpressiveInterjection("uh uh!")}",
        };

        private static readonly List<string> DysfluencyPositive = new List<string>()
        {
            $"{ExpressiveInterjection("ahem!")}",
            $"{ExpressiveInterjection("uh!")}",
            $"{ExpressiveInterjection("uh huh!")}",
            $"{ExpressiveInterjection("um!")}",
            $"{ExpressiveInterjection("hmm!")}",
        };

        protected static void PersonNotRecognized(StringBuilder speech)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.DYSFLUENCY_NEGATIVE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(GetSpeechPrefix(SpeechPrefix.REPOSE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("I don't recognize the current user.");
            speech.Append("Please go to the plugin configuration and link emby account personalization.");
            speech.Append("Or ask for help.");
        }
        protected static void OnLaunch(StringBuilder speech)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.GREETINGS));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("What media can I help you find.");
        }
        protected static void NotUnderstood(StringBuilder speech)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.DYSFLUENCY_NEGATIVE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("I misunderstood what you said.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            if (!(RandomIndex.NextDouble() > 0.7)) return;
            speech.Append(SayWithEmotion("Try using the type of media in your request.", Emotion.excited, Intensity.low));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("For example: Use the word \"Movie\" if the item is a movie, or \"Series\" if it is a TV series.");
        }
        protected static void NoItemExists(StringBuilder speech)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.DYSFLUENCY_NEGATIVE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("I was unable to find that item in the library.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("I may have misunderstood you.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Please Try again.");
        }
        protected static void ItemBrowse(StringBuilder speech, BaseItem item, IAlexaSession session, bool deviceAvailable = true)
        {
            if (deviceAvailable == false)
            {
                speech.Append(GetSpeechPrefix(SpeechPrefix.DYSFLUENCY_NEGATIVE));
                speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                speech.Append("That device is currently unavailable.");
                return;
            }

            //speech.Append(GetSpeechPrefix(SpeechPrefix.COMPLIANCE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            if (RandomIndex.NextDouble() > 0.5 && !item.IsFolder) //Randomly incorporate "Here is.." into phrasing
            {
                speech.Append("Here is ");
            }

            var name = StringNormalization.ValidateSpeechQueryString(item.Name);
            speech.Append(name);

            if (!item.IsFolder) //Don't describe a rating of a library or collection folder.
            {
                var rating = item.OfficialRating;
                speech.Append(", ");
                speech.Append(string.IsNullOrEmpty(rating) ? "unrated" : $"Rated {rating}");
            }

            if (!session.hasRoom) return;
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
        }
        protected static void BrowseNextUpEpisode(StringBuilder speech, BaseItem item, IAlexaSession session)
        {
            var season = item.Parent;
            var seriesName = season?.Parent.Name;
            speech.Append("Here is the next up episode for ");
            speech.Append(seriesName);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(item.Name);
            if (session.room is null) return;
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
        }
        protected static void NoNextUpEpisodeAvailable(StringBuilder speech)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("There doesn't seem to be a new episode available for that series.");
        }
        protected static void PlayNextUpEpisode(StringBuilder speech, BaseItem item, IAlexaSession session)
        {
            speech.Append("Playing the next up episode for ");
            speech.Append(item.Parent.Parent.Name);
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
        }
        protected static void ParentalControlNotAllowed(StringBuilder speech, BaseItem item, IAlexaSession session)
        {
            speech.Append("<say-as interpret-as=\"interjection\">mm hmm</say-as>");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Are you sure you are allowed access to ");
            speech.Append(item is null ? "this item" : item.Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayName(session.context.System.person));
            speech.Append("?");
        }
        protected static void PlayItem(StringBuilder speech, BaseItem item)
        {
            speech.Append("Now playing the ");
            speech.Append(item.ProductionYear);
            speech.Append(" ");
            speech.Append(item.GetType().Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(item.Name);
        }
        protected static void RoomContext(StringBuilder speech)
        {
            speech.Append(SayWithEmotion("I didn't get the room ", Emotion.disappointed, Intensity.high));
            speech.Append("that you wish to view that.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayWithEmotion("Which room did you want?", Emotion.excited, Intensity.medium));
        }
        protected static void VoiceAuthenticationExists(StringBuilder speech, IAlexaSession session)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.DYSFLUENCY_NEGATIVE));
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("This profile is already linked to an account.");
            speech.Append(SayName(session.context.System.person));
            speech.Append("'s account");

            

        }
        protected static void VoiceAuthenticationAccountLinkError(StringBuilder speech)
        {
            speech.Append("I was unable to learn your voice. ");
            speech.Append("Please make sure you have allowed personalization in the Alexa app.");
        }
        protected static void VoiceAuthenticationAccountLinkSuccess(StringBuilder speech, IAlexaSession session)
        {
            speech.Append(ExpressiveInterjection("Success "));
            speech.Append(ExpressiveInterjection(SayName(session.context.System.person)));
            speech.Append("!");
            speech.Append("Please look at the plugin configuration. ");
            speech.Append("You should now see the I.D. linked to your voice.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Choose your emby account name and press save. ");
        }
        protected static void UpComingEpisodes(StringBuilder speech, List<BaseItem> items, DateTime date)
        {
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" upcoming episode");
            speech.Append(items?.Count > 1 ? "s" : "");
            
            speech.Append($" scheduled to air over the next {(date - DateTime.Now).Days} days.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));

            var schedule = items.DistinctBy(item => item.Parent.ParentId).ToList();

            var dateGroup = schedule.GroupBy(s => s?.PremiereDate);

            foreach (var d in dateGroup)
            {
                // ReSharper disable once PossibleNullReferenceException
                // ReSharper disable once PossibleInvalidOperationException
                speech.Append($"On {d.Key.Value.DayOfWeek}'s:");
                speech.Append(InsertStrengthBreak(StrengthBreak.strong));
                var i = 1;
                foreach (var item in d)
                {
                    speech.Append(StringNormalization.ValidateSpeechQueryString(item.Parent.Parent.Name));
                    if (item.IndexNumber == 1)
                    {
                        speech.Append($" which will premiere season {SayAsCardinal(item.Parent.IndexNumber.ToString())} ");
                        speech.Append(SayAsDate(Date.md, d.Key.Value.ToString("M/d")));
                        speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    }

                    speech.Append(", ");
                    if (d.Count() > 1 && i == d.Count() - 1) speech.Append(" and ");
                    
                    i++;
                }
            }

            speech.Append(
                $"This completes the list of episodes scheduled for the next {(date - DateTime.Now).Days} days. ");

        }
        protected static void NewLibraryItems(StringBuilder speech, List<BaseItem> items, DateTime date, IAlexaSession session)
        {
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" new ");
            speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);

            //var date = DateTime.Parse(query.args[0]);
            speech.Append($" added in the past {(date - DateTime.Now).Days * -1} days. ");

            if (!session.supportsApl)
            {
                speech.Append(string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
            }
        }
        protected static void BrowseItemByActor(StringBuilder speech, List<BaseItem> actors)
        {
            speech.Append(GetSpeechPrefix(SpeechPrefix.COMPLIANCE));
            speech.Append("Items starring");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(string.Join(", ", actors));
        }

        // Use this to Randomize responses. 
        private static readonly Random RandomIndex = new Random();

        public static string GetSpeechPrefix(SpeechPrefix prefix)
        {
            switch (prefix)
            {
                case SpeechPrefix.COMPLIANCE: return Compliance[RandomIndex.Next(0, Compliance.Count)];
                case SpeechPrefix.APOLOGETIC: return SayWithEmotion(Apologetic[RandomIndex.Next(0, Apologetic.Count)], Emotion.disappointed, Intensity.medium);
                case SpeechPrefix.REPOSE: return Repose[RandomIndex.Next(0, Repose.Count)];
                case SpeechPrefix.GREETINGS: return Greetings[RandomIndex.Next(0, Greetings.Count)];
                case SpeechPrefix.DYSFLUENCY_POSITIVE: return DysfluencyPositive[RandomIndex.Next(0, DysfluencyPositive.Count)];
                case SpeechPrefix.DYSFLUENCY_NEGATIVE: return DysfluencyNegative[RandomIndex.Next(0, DysfluencyNegative.Count)];
                case SpeechPrefix.NONE: return string.Empty;
                case SpeechPrefix.DEFAULT: return string.Empty;
                default: return string.Empty;
            }
        }
    }
    public enum SpeechPrefix
    {
        REPOSE,
        APOLOGETIC,
        COMPLIANCE,
        NONE,
        GREETINGS,
        DYSFLUENCY_POSITIVE,
        DYSFLUENCY_NEGATIVE,
        DEFAULT
    }
}

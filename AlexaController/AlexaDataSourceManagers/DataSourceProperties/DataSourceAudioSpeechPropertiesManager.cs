using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Extensions;

namespace AlexaController.AlexaDataSourceManagers.DataSourceProperties
{
    public class DataSourceAudioSpeechPropertiesManager : Ssml
    {
        private enum SpeechPrefix
        {
            REPOSE,
            APOLOGETIC,
            COMPLIANCE,
            NONE,
            GREETINGS,
            NON_COMPLIANT,
            DEFAULT
        }

        // Use this to Randomize responses. Don't say things all the time.
        private static readonly Random RandomIndex = new Random();

        public static DataSourceAudioSpeechPropertiesManager Instance { get; private set; }

        public DataSourceAudioSpeechPropertiesManager()
        {
            Instance = this;
        }

        private readonly List<string> Apologetic = new List<string>()
        {
            "I'm Sorry about this.",
            "Apologies.",
            "I apologize.",
            "I'm Sorry.",
            ""
        };
        private readonly List<string> Compliance = new List<string>()
        {
            ExpressiveInterjection("OK"),
            ExpressiveInterjection("absolutely "),
            "Alright, ",
            SayWithEmotion("Yes, ... ", Emotion.excited, Intensity.medium),
            "Here you go.",
            ""
        };
        private readonly List<string> Repose     = new List<string>()
        {
            "One moment...",
            "One moment please...",
            $"Just {InsertVoicePitch(Pitch.high, "one")} moment...",
            "Just a moment...",
            "I can do that... just a moment...",
            ""
        };
        private readonly List<string> Greetings  = new List<string>()
        {
            "Hey",
            "Hi",
            "Hello",
            ""
        };
        private readonly List<string> Dysfluency = new List<string>()
        {
            "oh...",
            "umm... ",
            $"{ExpressiveInterjection("hmm")}...",
        };
        
        public async Task<Properties<string>> PersonNotRecognized()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.REPOSE));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("I don't recognize the current user.");
            speech.Append("Please go to the plugin configuration and link emby account personalization.");
            speech.Append("Or ask for help.");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> OnLaunch()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.GREETINGS));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("What media can I help you find.");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> NotUnderstood()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("I misunderstood what you said.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> NoItemExists()
        {
            var speech   = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("I was unable to find that item in the library.");

            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });

        }
        
        public async Task<Properties<string>> ItemBrowse(BaseItem item, IAlexaSession session, bool correctUserPhrasing = false, bool deviceAvailable = true)
        {
            var speech = new StringBuilder();

            if (deviceAvailable == false)
            {
                speech.Append("That device is currently unavailable.");
            }

            if (correctUserPhrasing && RandomIndex.NextDouble() < 0.3)
            {
                CorrectPhrasing(speech, session, item);
            }

            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            if (RandomIndex.NextDouble() > 0.7 && !item.IsFolder) //Don't describe the type of a library or collection folder.
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

            if (!session.hasRoom)
            {
                return await Task.FromResult(new Properties<string>()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                });
            }
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);

            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> BrowseNextUpEpisode(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            var season = item.Parent;
            var seriesName = season?.Parent.Name;
            speech.Append("Here is the next up episode for ");
            speech.Append(seriesName);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(item.Name);
            if (session.room is null)
            {
                return await Task.FromResult(new Properties<string>()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                });
            }
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
       
        public async Task<Properties<string>> PlayNextUpEpisode(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append("Playing the next up episode for ");
            speech.Append(item.Parent.Parent.Name);
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> ParentalControlNotAllowed(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
            speech.Append("Are you sure you are allowed access to ");
            speech.Append(item is null ? "this item" : item.Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayName(session.person));
            speech.Append("?");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> PlayItem(BaseItem item)
        {
            var speech = new StringBuilder();
            speech.Append("Now playing the ");
            speech.Append(item.ProductionYear);
            speech.Append(" ");
            speech.Append(item.GetType().Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(item.Name);
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> RoomContext()
        {
            var speech = new StringBuilder();
            //speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append(SayWithEmotion("I didn't get the room ", Emotion.disappointed, Intensity.high));
            speech.Append("that you wish to view that.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayWithEmotion("Which room did you want?", Emotion.excited, Intensity.medium));
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }
        
        public async Task<Properties<string>> VoiceAuthenticationExists(IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("This profile is already linked to ");
            speech.Append(SayName(session.person));
            speech.Append("'s account");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> VoiceAuthenticationAccountLinkError()
        {
            var speech = new StringBuilder();
            speech.Append("I was unable to learn your voice.");
            speech.Append("Please make sure you have allowed personalization in the Alexa app.");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> VoiceAuthenticationAccountLinkSuccess(IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(ExpressiveInterjection("Success "));
            speech.Append(ExpressiveInterjection(SayName(session.person)));
            speech.Append("!");
            speech.Append("Please look at the plugin configuration.");
            speech.Append("You should now see the I.D. linked to your voice.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Choose your emby account name and press save.");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> UpComingEpisodes(List<BaseItem> items, DateTime date)
        {
            var speech = new StringBuilder();
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" upcoming episode");
            speech.Append(items?.Count > 1 ? "s" : "");

            //var date = DateTime.Parse(query.args[0]);

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
                        //speech.Append(" and ");
                    }
                    speech.Append(", ");
                    if (d.Count() > 1 && i == d.Count() - 1)
                    {
                        speech.Append(" and ");
                    }

                    i++;
                }
            }
            speech.Append(
                $"This completes the list of episodes scheduled for the next {(date - DateTime.Now).Days} days. ");

            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> NewLibraryItems(List<BaseItem> items, DateTime date, IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" new ");
            speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);

            //var date = DateTime.Parse(query.args[0]);
            speech.Append($" added in the past {(date - DateTime.Now).Days * -1} days. ");

            if(!session.supportsApl){
                speech.Append(string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}",
                // ReSharper disable once AssignNullToNotNullAttribute
                items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
            }
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> NoNextUpEpisodeAvailable()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("There doesn't seem to be a new episode available for that series.");
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        public async Task<Properties<string>> BrowseItemByActor(List<BaseItem> actors)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.COMPLIANCE));
            speech.Append("Items starring");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(string.Join(", ", actors));
            //speech.Append(string.Join(", and", actors, actors.Count -1, 1));
            return await Task.FromResult(new Properties<string>()
            {
                value = speech.ToString(),
                audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
            });
        }

        private static void CorrectPhrasing(StringBuilder speech, IAlexaSession session, BaseItem item)
        {
            speech.Append("Hey!");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayName(session.person));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("listen");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("To make searching media items faster.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Try using the item type in your request");
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append($"For example, using the phrase: \"Show the {item.GetType().Name}\" ");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append($"{item.Name}");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("will help me find your selection more efficiently.");
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
        }

        private string GetSpeechPrefix(SpeechPrefix prefix)
        {
            switch (prefix)
            {
                case SpeechPrefix.COMPLIANCE: return Compliance[RandomIndex.Next(0, Compliance.Count)];
                case SpeechPrefix.APOLOGETIC: return SayWithEmotion(Apologetic[RandomIndex.Next(0, Apologetic.Count)], Emotion.disappointed, Intensity.medium);
                case SpeechPrefix.REPOSE: return Repose[RandomIndex.Next(0, Repose.Count)];
                case SpeechPrefix.GREETINGS: return Greetings[RandomIndex.Next(0, Greetings.Count)];
                case SpeechPrefix.NON_COMPLIANT: return Dysfluency[RandomIndex.Next(0, Dysfluency.Count)];
                case SpeechPrefix.NONE: return string.Empty;
                case SpeechPrefix.DEFAULT: return string.Empty;
                default: return string.Empty;
            }
        }
    }
}

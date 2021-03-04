using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation.DataSources;
using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Api;
using AlexaController.DataSourceProperties.AplaDataSourceProperties;
using AlexaController.Session;
using AlexaController.Utils;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Extensions;

namespace AlexaController
{
    public class AplaDataSourceManager : Ssml
    {

        private static readonly Random RandomIndex = new Random();

        public static  AplaDataSourceManager Instance { get; set; }
        
        public AplaDataSourceManager()
        {
            Instance = this;
        }

        private readonly List<string> Apologetic = new List<string>()
        {
            "I'm Sorry about this.",
            "Apologies.",
            "I apologize.",
            "I'm sorry about this.",
            "I'm Sorry.",
            ""
        };
        private readonly List<string> Compliance = new List<string>()
        {
            ExpressiveInterjection("OK"),
            "Alright, ",
            SayWithEmotion("Yes, ... ", Emotion.excited, Intensity.medium),
            "Yep, "
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
            $"{ExpressiveInterjection("wow")}...",
        };

        public async Task<IDataSource> PersonNotRecognized()
        {
            var speech = new StringBuilder();
            speech.Append("I don't recognize the current user.");
            speech.Append("Please go to the plugin configuration and link emby account personalization.");
            speech.Append("Or ask for help.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> OnLaunch()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.GREETINGS));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("What media can I help you find.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13",
                }
            };
        }

        public async Task<IDataSource> NotUnderstood()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("I misunderstood what you said.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> NoItemExists(IAlexaSession session, string requestItem)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            var baseItem = session.NowViewingBaseItem;
            var name = StringNormalization.ValidateSpeechQueryString(baseItem.Name);
            var index = baseItem.IndexNumber;
            speech.Append(SpeechRate(Rate.fast, SayWithEmotion(name, Emotion.disappointed, Intensity.high)));
            speech.Append(ExpressiveInterjection(" doesn't contain "));
            speech.Append(requestItem);
            speech.Append(index);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };

        }

        public async Task<IDataSource> ItemBrowse(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            var name = StringNormalization.ValidateSpeechQueryString(item.Name);
            var rating = item.OfficialRating;
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

            if (session.room is null)
            {
                return new DataSource()
                {
                    properties = new SpeechContentProperties()
                    {
                        value = speech.ToString(),
                        audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                    }
                };
            }
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> BrowseNextUpEpisode(BaseItem item, IAlexaSession session)
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
                return new DataSource()
                {
                    properties = new SpeechContentProperties()
                    {
                        value = speech.ToString(),
                        audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                    }
                };
            }
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }
        
        public async Task<IDataSource> DisplayMovieCollection(BaseItem item)
        {
            var speech = new StringBuilder();
            speech.Append("The ");
            speech.Append(item.Name);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> BrowseLibrary(BaseItem item)
        {
            var speech = new StringBuilder();
            speech.Append("Here is the ");
            speech.Append(item.Name);
            speech.Append(" library.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> ParentalControlNotAllowed(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
            speech.Append("Are you sure you are allowed access to ");
            speech.Append(item is null ? "this item" : item.Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayName(session.person));
            speech.Append("?");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/musical/amzn_sfx_electronic_beep_02"
                }
            };
        }

        public async Task<IDataSource> PlayItem(BaseItem item)
        {
            var speech = new StringBuilder();
            speech.Append("Now playing the ");
            speech.Append(item.ProductionYear);
            speech.Append(" ");
            speech.Append(item.GetType().Name);
            speech.Append(InsertStrengthBreak(StrengthBreak.weak)); 
            speech.Append(item.Name);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> RoomContext()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append(SayInDomain(Domain.conversational, "I didn't get the room you wish to view that."));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(SayInDomain(Domain.music, "Which room did you want?"));
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl =  "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> PlayNextUpEpisode(BaseItem item, IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append("Playing the next up episode for ");
            speech.Append(item.Parent.Parent.Name);
            speech.Append("Showing in the ");
            speech.Append(session.room.Name);
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> GenericItemDoesNotExists()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append(SayWithEmotion("That item ", Emotion.disappointed, Intensity.high));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(ExpressiveInterjection("doesn't exist "));
            speech.Append("in the library!");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> NoDeviceConfiguration(IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("There is no device configuration for ");
            speech.Append(session.room);
            speech.Append("Please look in the plugin configuration to map rooms to emby ready devices.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> DeviceUnavailable(string deviceName)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append(SayWithEmotion("I was unable to access ", Emotion.disappointed, Intensity.medium));
            speech.Append(SayWithEmotion("the device in the ", Emotion.disappointed, Intensity.medium));
            speech.Append(SayWithEmotion(deviceName, Emotion.disappointed, Intensity.medium));
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> VoiceAuthenticationExists(IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
            speech.Append(InsertStrengthBreak(StrengthBreak.strong));
            speech.Append("This profile is already linked to ");
            speech.Append(SayName(session.person));
            speech.Append("'s account");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl =  "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> VoiceAuthenticationAccountLinkError()
        {
            var speech = new StringBuilder();
            speech.Append("I was unable to learn your voice.");
            speech.Append("Please make sure you have allowed personalization in the Alexa app.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> VoiceAuthenticationAccountLinkSuccess(IAlexaSession session)
        {
            var speech = new StringBuilder();
            speech.Append(ExpressiveInterjection("Success "));
            speech.Append(ExpressiveInterjection(SayName(session.person)));
            speech.Append("!");
            speech.Append("Please look at the plugin configuration.");
            speech.Append("You should now see the I.D. linked to your voice.");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append("Choose your emby account name and press save.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }
        

        public async Task<IDataSource> UpComingEpisodes(List<BaseItem> items, DateTime date)
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

            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> NewItemsAplaOnly(List<BaseItem> items, DateTime date)
        {
            var speech = new StringBuilder();
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" new ");
            speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);

            //var date = DateTime.Parse(query.args[0]);
            speech.Append($" added in the past {(date - DateTime.Now).Days * -1} days. ");

            speech.Append(string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}",
                // ReSharper disable once AssignNullToNotNullAttribute
                items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> GetNewItemsApl(List<BaseItem> items, DateTime date)
        {
            var speech = new StringBuilder();
            speech.Append("There ");
            speech.Append(items?.Count > 1 ? "are" : "is");
            speech.Append(SayAsCardinal(items?.Count.ToString()));
            speech.Append(" new ");
            speech.Append(items?.Count > 1 ? items[0].GetType().Name + "s" : items?[0].GetType().Name);

            //var date = DateTime.Parse(.args[0]);
            speech.Append($" added in the past {(date - DateTime.Now).Days *-1} days.");

            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> NoNextUpEpisodeAvailable()
        {
            var speech = new StringBuilder();
            speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
            speech.Append("There doesn't seem to be a new episode available for that series.");
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }

        public async Task<IDataSource> BrowseItemByActor(List<BaseItem> actors)
        {
            var speech = new StringBuilder();
            speech.Append("Items starring");
            speech.Append(InsertStrengthBreak(StrengthBreak.weak));
            speech.Append(string.Join(", ", actors));
            speech.Append(string.Join(", and", actors, actors.Count -1, 1));
            return new DataSource()
            {
                properties = new SpeechContentProperties()
                {
                    value = speech.ToString(),
                    audioUrl = "soundbank://soundlibrary/computers/beeps_tones/beeps_tones_13"
                }
            };
        }
        
        private string GetSpeechPrefix(SpeechPrefix prefix)
        {
            switch (prefix)
            {
                case SpeechPrefix.COMPLIANCE    : return Compliance[RandomIndex.Next(0, Compliance.Count)];
                case SpeechPrefix.APOLOGETIC    : return SayWithEmotion(Apologetic[RandomIndex.Next(0, Apologetic.Count)], Emotion.disappointed, Intensity.medium);
                case SpeechPrefix.REPOSE        : return Repose[RandomIndex.Next(0, Repose.Count)];
                case SpeechPrefix.GREETINGS     : return Greetings[RandomIndex.Next(0, Greetings.Count)];
                case SpeechPrefix.NON_COMPLIANT : return Dysfluency[RandomIndex.Next(0, Dysfluency.Count)];
                case SpeechPrefix.NONE          : return string.Empty;
                case SpeechPrefix.DEFAULT       : return string.Empty;
                default                          : return string.Empty;
            }
        }
    }
}

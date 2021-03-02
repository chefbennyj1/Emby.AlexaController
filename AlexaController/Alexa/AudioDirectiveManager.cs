using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AlexaController.Alexa.Presentation;
using AlexaController.Alexa.Presentation.APLA.Components;
using AlexaController.Alexa.SpeechSynthesis;
using AlexaController.Api.ResponseModel;
using AlexaController.Utils;
using MediaBrowser.Model.Extensions;
using Document = AlexaController.Alexa.Presentation.APLA.Document;

namespace AlexaController.Alexa
{

    public class AudioDirectiveManager : Ssml
    {
        public static AudioDirectiveManager Instance { get; private set; }
        
        public AudioDirectiveManager()
        {
            Instance = this;
        }

        private static readonly Random RandomIndex = new Random();
        
        public async Task<IDirective> GetAudioDirectiveAsync(AudioDirectiveQuery query)
        {
            return await Task.FromResult(new Directive()
            {
                type     = Directive.AplaRenderDocument,
                token    = query.speechPrefix.ToString(),
                document = new Document()
                {
                    mainTemplate = new MainTemplate()
                    {
                        parameters = new List<string>() { "payload" },
                        item = new Mixer()
                        {
                            items = new List<AudioBaseItem>()
                            {
                                new Speech() { content = $"<speak>{GetSpeechPhrase(query)}</speak>" },
                                query.audio
                            }
                        }
                        
                    }
                }
            });
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
        
        private string GetSpeechPhrase(AudioDirectiveQuery query)
        {
            var speech = new StringBuilder();
            
            switch (query.speechContent)
            {
                case SpeechContent.PERSON_NOT_RECOGNIZED:
                {
                    speech.Append("I don't recognize the current user.");
                    speech.Append("Please go to the plugin configuration and link emby account personalization.");
                    speech.Append("Or ask for help.");
                    return speech.ToString();
                }

                case SpeechContent.PROGRESSIVE_RESPONSE:
                {
                        //speech.Append(GetRandomSemanticSpeechResponse(SpeechType.COMPLIANCE));
                        //speech.Append(Ssml.InsertStrengthBreak(StrengthBreak.weak));
                        //speech.Append(GetRandomSemanticSpeechResponse(SpeechType.REPOSE));
                        return "";
                    }

                case SpeechContent.ON_LAUNCH:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.GREETINGS));
                    speech.Append(InsertStrengthBreak(StrengthBreak.strong));
                    speech.Append("What media can I help you find.");
                    return speech.ToString();
                }

                case SpeechContent.NOT_UNDERSTOOD:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append("I misunderstood what you said.");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayWithEmotion("Can you say that again?", Emotion.excited, Intensity.low));
                    return speech.ToString();
                }

                case SpeechContent.NO_SEASON_ITEM_EXIST:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    var name = StringNormalization.ValidateSpeechQueryString(query.session?.NowViewingBaseItem.Name);
                    speech.Append(SpeechRate(Rate.fast, SayWithEmotion(name, Emotion.disappointed, Intensity.high)));
                    speech.Append(ExpressiveInterjection(" doesn't contain "));
                    speech.Append(" season ");
                    speech.Append(query.args?[0]);
                    return speech.ToString();
                }

                case SpeechContent.BROWSE_ITEM:
                {
                    var name = StringNormalization.ValidateSpeechQueryString(query.items?[0].Name);
                    var rating = query.items?[0].OfficialRating;
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

                    if (query.session.room is null)
                    {
                        return speech.ToString();
                    }
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(query.session.room.Name);
                    return speech.ToString();
                }

                case SpeechContent.BROWSE_NEXT_UP_EPISODE:
                {
                    var season = query.items?[0].Parent;
                    var seriesName = season?.Parent.Name;
                    speech.Append("Here is the next up episode for ");
                    speech.Append(seriesName);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(query.items?[0].Name);
                    if (query.session.room is null)
                    {
                        return speech.ToString();
                    }
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Showing in the ");
                    speech.Append(query.session.room.Name);
                    return speech.ToString();
                }

                case SpeechContent.DISPLAY_MOVIE_COLLECTION:
                {
                    speech.Append("The ");
                    speech.Append(query.items?[0].Name);
                    return speech.ToString();
                }

                case SpeechContent.BROWSE_LIBRARY:
                {
                    speech.Append("Here is the ");
                    speech.Append(query.items?[0].Name);
                    speech.Append(" library.");
                    return speech.ToString();
                }

                case SpeechContent.PARENTAL_CONTROL_NOT_ALLOWED:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
                    speech.Append("Are you sure you are allowed access to ");
                    speech.Append(query.items is null ? "this item" : query.items[0].Name);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayName(query.session.person));
                    speech.Append("?");
                    return speech.ToString();
                }

                case SpeechContent.PLAY_MEDIA_ITEM:
                {
                    speech.Append("Now playing the ");
                    speech.Append(query.items?[0].ProductionYear);
                    speech.Append(" ");
                    speech.Append(query.items?[0].GetType().Name);
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak)); 
                    speech.Append(query.items?[0].Name);
                    return speech.ToString();
                }

                case SpeechContent.ROOM_CONTEXT:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append(SayInDomain(Domain.conversational, "I didn't get the room you wish to view that."));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(SayInDomain(Domain.music, "Which room did you want?"));
                    return speech.ToString();
                }

                case SpeechContent.PLAY_NEXT_UP_EPISODE:
                {
                    speech.Append("Playing the next up episode for ");
                    speech.Append(query.items?[0].Parent.Parent.Name);
                    speech.Append("Showing in the ");
                    speech.Append(query.session.room.Name);
                    return speech.ToString();
                }

                case SpeechContent.GENERIC_ITEM_NOT_EXISTS_IN_LIBRARY:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append(SayWithEmotion("That item ", Emotion.disappointed, Intensity.high));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(ExpressiveInterjection("doesn't exist "));
                    speech.Append("in the library!");
                    return speech.ToString();
                }

                case SpeechContent.NO_DEVICE_CONFIGURATION:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append("There is no device configuration for ");
                    speech.Append(query.session.room);
                    speech.Append("Please look in the plugin configuration to map rooms to emby ready devices.");
                    return speech.ToString();
                }

                case SpeechContent.DEVICE_UNAVAILABLE:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append(SayWithEmotion("I was unable to access ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SayWithEmotion("the device in the ", Emotion.disappointed, Intensity.medium));
                    speech.Append(SayWithEmotion(query.args?[0], Emotion.disappointed, Intensity.medium));
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    return speech.ToString();
                }

                case SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_EXISTS:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.NON_COMPLIANT));
                    speech.Append(InsertStrengthBreak(StrengthBreak.strong));
                    speech.Append("This profile is already linked to ");
                    speech.Append(SayName(query.session.person));
                    speech.Append("'s account");
                    return speech.ToString();
                }

                case SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_LINK_ERROR:
                {
                    speech.Append("I was unable to learn your voice.");
                    speech.Append("Please make sure you have allowed personalization in the Alexa app.");
                    return speech.ToString();
                }

                case SpeechContent.VOICE_AUTHENTICATION_ACCOUNT_LINK_SUCCESS:
                {
                    speech.Append(ExpressiveInterjection("Success "));
                    speech.Append(ExpressiveInterjection(SayName(query.session.person)));
                    speech.Append("!");
                    speech.Append("Please look at the plugin configuration.");
                    speech.Append("You should now see the I.D. linked to your voice.");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append("Choose your emby account name and press save.");
                    return speech.ToString();
                }

                case SpeechContent.UP_COMING_EPISODES:
                {
                    speech.Append("There ");
                    speech.Append(query.items?.Count > 1 ? "are" : "is");
                    speech.Append(SayAsCardinal(query.items?.Count.ToString()));
                    speech.Append(" upcoming episode");
                    speech.Append(query.items?.Count > 1 ? "s" : "");
                    
                    var date = DateTime.Parse(query.args[0]);
                    
                    speech.Append($" scheduled to air over the next {(date - DateTime.Now).Days} days.");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                   
                    var schedule = query.items.DistinctBy(item => item.Parent.ParentId).ToList();

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
                            if (d.Count() > 1 && i == d.Count() -1)
                            {
                                speech.Append(" and ");
                            }
                            //speech.Append(" will air on ");
                            //speech.Append($"{item.PremiereDate.Value.DayOfWeek}'s");
                            
                            i++;
                        }
                    }


                    
                    
                    speech.Append(
                        $"This completes the list of episodes scheduled for the next {(date - DateTime.Now).Days} days. ");

                    return speech.ToString();
                }

                case SpeechContent.NEW_ITEMS_DISPLAY_NONE:
                {
                    speech.Append("There ");
                    speech.Append(query.items?.Count > 1 ? "are" : "is");
                    speech.Append(SayAsCardinal(query.items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(query.items?.Count > 1 ? query.items[0].GetType().Name + "s" : query.items?[0].GetType().Name);

                    var date = DateTime.Parse(query.args[0]);
                    speech.Append($" added in the past {(date - DateTime.Now).Days * -1} days. ");

                    speech.Append(string.Join($", {InsertStrengthBreak(StrengthBreak.weak)}",
                        // ReSharper disable once AssignNullToNotNullAttribute
                        query.items?.ToArray().Select(item => StringNormalization.ValidateSpeechQueryString(item.Name))));
                    return speech.ToString();
                }

                case SpeechContent.NEW_ITEMS_APL:
                {
                    speech.Append("There ");
                    speech.Append(query.items?.Count > 1 ? "are" : "is");
                    speech.Append(SayAsCardinal(query.items?.Count.ToString()));
                    speech.Append(" new ");
                    speech.Append(query.items?.Count > 1 ? query.items[0].GetType().Name + "s" : query.items?[0].GetType().Name);

                    var date = DateTime.Parse(query.args[0]);
                    speech.Append($" added in the past {(date - DateTime.Now).Days *-1} days.");

                    return speech.ToString();
                }

                case SpeechContent.NO_NEXT_UP_EPISODE_AVAILABLE:
                {
                    speech.Append(GetSpeechPrefix(SpeechPrefix.APOLOGETIC));
                    speech.Append("There doesn't seem to be a new episode available for that series.");
                    return speech.ToString();
                }

                case SpeechContent.BROWSE_ITEMS_BY_ACTOR:
                    speech.Append("Items starring");
                    speech.Append(InsertStrengthBreak(StrengthBreak.weak));
                    speech.Append(query.args[0]);
                    return speech.ToString();
                default:
                    throw new ArgumentOutOfRangeException(nameof(AudioDirectiveManager), query.speechContent, null);
            }
        }

    }
}

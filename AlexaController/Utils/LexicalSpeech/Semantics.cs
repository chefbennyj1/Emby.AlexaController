using System;
using System.Collections.Generic;
using AlexaController.Alexa.RequestData.Model;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Alexa.Speech;

// ReSharper disable ComplexConditionExpression
// ReSharper disable InconsistentNaming

namespace AlexaController.Utils.LexicalSpeech
{
   

    public enum SpeechType
    {
        REPOSE,
        APOLOGETIC,
        COMPLIANCE,
        NONE,
        GREETINGS,
        NON_COMPLIANT
    }

    public class Semantics : OutputSpeech
    {
        /*
         * Add empty strings to each Semantic Phrase list so that, sometimes, Alexa says nothing.
         */

        protected static string GetRandomSemanticSpeechResponse(SpeechType type)
        {
            switch (type.ToString())
            {
                case "COMPLIANCE"    : return GetCompliance();
                case "APOLOGETIC"    : return GetSpeechApology();
                case "REPOSE"        : return GetRepose();
                case "NONE"          : return string.Empty;
                case "GREETINGS"     : return GetGreeting();
                case "NON_COMPLIANT" : return GetNonCompliance();
                default              : return string.Empty;
            }
        }

        private static readonly List<string> Repose       = new List<string>
        {
            "One moment...",
            "One moment please...",
            "Just one moment...",
            "Just a moment...",
            "I can do that... just a moment...",
            ""
        };

        private static readonly List<string> Apologetic   = new List<string>
        {
            "I'm Sorry about this.",
            "Apologies.",
            "I apologize.",
            "I'm sorry about this.",
            "I'm Sorry.",
            ""
        };

        private static readonly List<string> Apologetic2  = new List<string>()
        {
            "hoh...",
            "hmm...",
            ""
        };

        private static readonly List<string> Compliance   = new List<string>
        {
            "OK, ",
            "Alright, ",
            "Yes, ",
            "Yep, "
        };

        private static readonly List<string> NonCompliant = new List<string>
        {
            "No way, Hosea!... ",
            "No can do... ",
            "I can't do that... ",
            ""
        };

        private static readonly List<string> Dysfluency   = new List<string>()
        {
            "oh...",
            "um... ",
            ""
        };

        private static readonly List<string> Greetings    = new List<string>()
        {
            "Hey",
            "Hi",
            "Hello",
            ""
        };

        
        private static string GetSpeechApology()
        {
            var i = Plugin.RandomIndex.Next(1, 2);
            switch (i)
            {
                case 1:
                    return string.Join(" ", SpeechStyle.SpeechRate(Rate.slow, SpeechStyle.SayWithEmotion(Apologetic2[Plugin.RandomIndex.Next(1, Apologetic2.Count)], Emotion.disappointed, Intensity.low)), SpeechStyle.SayWithEmotion("ya know what?", Emotion.disappointed, Intensity.medium), SpeechStyle.InsertStrengthBreak(StrengthBreak.weak));
                case 2:
                    return $"{GetSpeechDysfluency(Emotion.disappointed, Rate.slow)}, {SpeechStyle.SayWithEmotion(Apologetic[Plugin.RandomIndex.Next(1, Apologetic.Count)], Emotion.disappointed, Intensity.medium)} {SpeechStyle.InsertStrengthBreak(StrengthBreak.weak)}";

            }
            return string.Empty;
        }

        private static string GetSpeechDysfluency(Emotion emotion, Rate rate) => SpeechStyle.SayWithEmotion(SpeechStyle.SpeechRate(rate, Dysfluency[Plugin.RandomIndex.Next(1, Dysfluency.Count)]), emotion, Intensity.medium);
        
        private static string GetTimeOfDayResponse()                          => DateTime.Now.Hour < 12 && DateTime.Now.Hour > 4 ? "Good morning" : DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17 ? "Good afternoon" : "Good evening";
        
        private static string GetCompliance()                                 => Compliance[Plugin.RandomIndex.Next(1, Compliance.Count)];
        
        private static string GetRepose()                                     => Repose[Plugin.RandomIndex.Next(1, Repose.Count)];
        
        private static string GetNonCompliance()                              => SpeechStyle.SayWithEmotion(NonCompliant[Plugin.RandomIndex.Next(1, NonCompliant.Count)], Emotion.disappointed, Intensity.low);
        
        private static string GetGreeting()
        {
            var i = Plugin.RandomIndex.Next(1, 2);

            switch (i)
            {
                case 1:
                    return $"{SpeechStyle.SayWithEmotion(Greetings[Plugin.RandomIndex.Next(1, Greetings.Count)], Emotion.excited, Intensity.low)} {SpeechStyle.InsertStrengthBreak(StrengthBreak.weak)}";
                case 2:
                    return GetTimeOfDayResponse();

            }
            return string.Empty;
        }


        public static string SayName(IPerson person) => $"<alexa:name type=\"first\" personId=\"{person.personId}\"/>";
    }
}
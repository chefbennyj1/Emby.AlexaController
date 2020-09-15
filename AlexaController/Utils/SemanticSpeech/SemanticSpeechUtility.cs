using System;
using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;

// ReSharper disable ComplexConditionExpression

namespace AlexaController.Utils.SemanticSpeech
{
    public enum SemanticSpeechType
    {
        REPOSE,
        APOLOGETIC,
        COMPLIANCE,
        NONE,
        GREETINGS,
        NON_COMPLIANT,
        
    }

    public class SemanticSpeechUtility : OutputSpeech
    {
        /*
         * Add empty strings to each Semantic Phrase list so that, sometimes, Alexa says nothing.
         */


        public static string GetSemanticSpeechResponse(SemanticSpeechType type)
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

        private static readonly List<string> Repose = new List<string>
        {
            "One moment...",
            "One moment please...",
            "Just one moment...",
            "Just a moment...",
            "I can do that... just a moment...",
            ""
        };

        private static readonly List<string> Apologetic = new List<string>
        {
            "I'm Sorry about this.",
            "Apologies.",
            "I apologize.",
            "I'm sorry about this.",
            "I'm Sorry.",
            ""
        };

        private static readonly List<string> Apologetic2 = new List<string>()
        {
            "hoh... ya know what?",
            "hmm... ya know what?",
            ""
        };

        private static readonly List<string> Compliance = new List<string>
        {
            "OK, ",
            "Alright, ",
            ""
        };

        private static readonly List<string> NonCompliant = new List<string>
        {
            "No way, Hosea!... ",
            "No can do... ",
            "I can't do that... ",
            ""
        };

        private static readonly List<string> Dysfluency = new List<string>()
        {
            "oh...",
            "um... ",
            ""
        };

        private static readonly List<string> Greetings  = new List<string>()
        {
            "Hey",
            "Hi",
            "Hello",
            ""
        };

        private static string GetSpeechDysfluency(Emotion emotion, Rate rate = Rate.slow)
        {
            var text = Dysfluency[Plugin.RandomIndex.Next(1, Dysfluency.Count)];
            return $"{SayWithEmotion(SpeechRate(rate, text), emotion, Intensity.medium)} {InsertStrengthBreak(StrengthBreak.strong)}";
        }

        private static string GetSpeechApology()
        {
            var i = Plugin.RandomIndex.Next(1, 2);
            switch (i)
            {
                case 1:
                    return $"{SpeechRate(Rate.slow, SayWithEmotion(Apologetic2[Plugin.RandomIndex.Next(1, Apologetic2.Count)], Emotion.disappointed, Intensity.low))} {InsertStrengthBreak(StrengthBreak.weak)}";
                case 2:
                    return $"{GetSpeechDysfluency(Emotion.disappointed)}, {SayWithEmotion(Apologetic[Plugin.RandomIndex.Next(1, Apologetic.Count)], Emotion.disappointed, Intensity.medium)} {InsertStrengthBreak(StrengthBreak.weak)}";

            }

            return string.Empty;
        }

        private static string GetTimeOfDayResponse()
        {
            var hour = DateTime.Now.Hour;
            return hour < 12 && hour > 4 ? "Good morning" : hour > 12 && hour < 17 ? "Good afternoon" : "Good evening";
        }

        private static string GetCompliance()
        {
            return $"{Compliance[Plugin.RandomIndex.Next(1, Compliance.Count)]}";
        }

        private static string GetRepose()
        {
            return $"{Repose[Plugin.RandomIndex.Next(1, Repose.Count)]} {InsertStrengthBreak(StrengthBreak.weak)}";
        }

        private static string GetNonCompliance()
        {
            return $"{SayWithEmotion(NonCompliant[Plugin.RandomIndex.Next(1, NonCompliant.Count)], Emotion.disappointed, Intensity.low)}";
        }

        private static string GetGreeting()
        {
            var i = Plugin.RandomIndex.Next(1, 2);

            switch (i)
            {
                case 1:
                    return $"{SayWithEmotion(Greetings[Plugin.RandomIndex.Next(1, Greetings.Count)], Emotion.excited, Intensity.low)} {InsertStrengthBreak(StrengthBreak.weak)}";
                case 2:
                    return GetTimeOfDayResponse();

            }

            return string.Empty;

        }
      
    }
}
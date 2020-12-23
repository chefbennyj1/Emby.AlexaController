using System;
using System.Collections.Generic;
using AlexaController.Alexa.ResponseData.Model;
using AlexaController.Alexa.SpeechSynthesisMarkupLanguage;

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

    public class Lexicons : OutputSpeech
    {
        /*
         * Add empty strings to each Semantic Phrase list so that, sometimes, Alexa says nothing.
         */

        private static readonly Random RandomIndex = new Random();

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
            "now now... ",
            ""
        };

        private static readonly List<string> Dysfluency   = new List<string>()
        {
            "oh...",
            "umm... "
        };

        private static readonly List<string> Greetings    = new List<string>()
        {
            "Hey",
            "Hi",
            "Hello",
            ""
        };
        
        private static string GetSpeechApology() => 
            RandomIndex.NextDouble() < 0.5 
                ? string.Join(" ", Ssml.SpeechRate(Rate.slow, Ssml.SayWithEmotion(Apologetic2[RandomIndex.Next(1, Apologetic2.Count)], Emotion.disappointed, Intensity.low)), 
                    Ssml.SayWithEmotion("ya know what?", Emotion.disappointed, Intensity.medium), 
                    Ssml.InsertStrengthBreak(StrengthBreak.weak)) 
                : string.Join(" ", GetSpeechDysfluency(Emotion.disappointed, Intensity.medium, Rate.slow), 
                    Ssml.SayWithEmotion(Apologetic[RandomIndex.Next(0, Apologetic.Count)], Emotion.disappointed, Intensity.medium), 
                    Ssml.InsertStrengthBreak(StrengthBreak.weak));

        protected static string GetSpeechDysfluency(Emotion emotion, Intensity intensity, Rate rate) => Ssml.SayWithEmotion(Ssml.SpeechRate(rate, Dysfluency[RandomIndex.Next(0, Dysfluency.Count)]), emotion, intensity);
        
        private static string GetTimeOfDayResponse() => DateTime.Now.Hour < 12 && DateTime.Now.Hour > 4 ? "Good morning" : DateTime.Now.Hour > 12 && DateTime.Now.Hour < 17 ? "Good afternoon" : "Good evening";
        
        private static string GetCompliance() => Compliance[RandomIndex.Next(0, Compliance.Count)];
        
        private static string GetRepose() => Repose[RandomIndex.Next(0, Repose.Count)];
        
        private static string GetNonCompliance() => Ssml.SayWithEmotion(NonCompliant[RandomIndex.Next(1, NonCompliant.Count)], Emotion.disappointed, Intensity.low);
        
        private static string GetGreeting() => 
            RandomIndex.NextDouble() < 0.5 
                ? string.Join(" ", Ssml.SayWithEmotion(Greetings[RandomIndex.Next(1, Greetings.Count)], Emotion.excited, Intensity.low), 
                Ssml.InsertStrengthBreak(StrengthBreak.weak)) 
                : GetTimeOfDayResponse();
        
        
    }
}
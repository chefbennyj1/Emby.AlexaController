using System;
using System.Text.RegularExpressions;

// ReSharper disable TooManyChainedReferences
// ReSharper disable TooManyDependencies
// ReSharper disable once UnusedAutoPropertyAccessor.Local
// ReSharper disable once ExcessiveIndentation
// ReSharper disable twice ComplexConditionExpression
// ReSharper disable PossibleNullReferenceException
// ReSharper disable TooManyArguments
// ReSharper disable once InconsistentNaming

namespace AlexaController.Utils
{
    public class DateTimeDurationNormalization
    {
        public class Duration
        {
            public int year   { get; set; }
            public int month  { get; set; }
            public int week   { get; set; }
            public int day    { get; set; }
            public int hour   { get; set; }
            public int minute { get; set; }
            public int second { get; set; }
        }

        public static DateTime GetMinDateCreation(Duration duration)
        {
            return DateTime.Now
                .AddYears(-duration.year)
                .AddMonths(-duration.month)
                .AddDays(-(duration.week*7))
                .AddDays(-duration.day)
                .AddHours(-duration.hour)
                .AddMinutes(-duration.minute)
                .AddSeconds(-duration.second);
        }

        public static Duration DeserializeDurationFromIso8601(string iso8601)
        {
            const string pattern = @"^P(?!$)(\d+(?:\.\d+)?Y)?(\d+(?:\.\d+)?M)?(\d+(?:\.\d+)?W)?(\d+(?:\.\d+)?D)?(T(?=\d)(\d+(?:\.\d+)?H)?(\d+(?:\.\d+)?M)?(\d+(?:\.\d+)?S)?)?$";
            var regex            = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matchCollection  = regex.Matches(iso8601);

            var yearMatchValue   = matchCollection[0].Groups[1].Value;
            var monthMatchValue  = matchCollection[0].Groups[2].Value;
            var weekMatchValue   = matchCollection[0].Groups[3].Value;
            var dayMatchValue    = matchCollection[0].Groups[4].Value;
            var hourMatchValue   = matchCollection[0].Groups[6].Value;
            var minuteMatchValue = matchCollection[0].Groups[7].Value;
            var secondMatchValue = matchCollection[0].Groups[8].Value;

            return new Duration()
            {
                year   = yearMatchValue   != string.Empty ? Convert.ToInt32(yearMatchValue.Replace(  "Y", string.Empty)) : 0,
                month  = monthMatchValue  != string.Empty ? Convert.ToInt32(monthMatchValue.Replace( "M", string.Empty)) : 0,
                week   = weekMatchValue   != string.Empty ? Convert.ToInt32(weekMatchValue.Replace(  "W", string.Empty)) : 0,
                day    = dayMatchValue    != string.Empty ? Convert.ToInt32(dayMatchValue.Replace(   "D", string.Empty)) : 0,
                hour   = hourMatchValue   != string.Empty ? Convert.ToInt32(hourMatchValue.Replace(  "H", string.Empty)) : 0,
                minute = minuteMatchValue != string.Empty ? Convert.ToInt32(minuteMatchValue.Replace("M", string.Empty)) : 0,
                second = secondMatchValue != string.Empty ? Convert.ToInt32(secondMatchValue.Replace("S", string.Empty)) : 0
            };

        }
    }
}

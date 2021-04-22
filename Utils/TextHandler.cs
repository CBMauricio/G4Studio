using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace G4Studio.Utils
{
    public static class TextHandler
    {
        public static string GetOnlyAlphabetChars(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            return Regex.Replace(value, "[^a-zA-Z0-9]", string.Empty);
        }
        public static string RemoveWhitespace(string input)
        {
            if (input is null) return string.Empty;

            return new string(input.ToCharArray()
                .Where(c => !Char.IsWhiteSpace(c))
                .ToArray());
        }

        public static string GetTimeFormattedFromMilseconds(long miliseconds)
        {
            TimeSpan t = TimeSpan.FromMilliseconds(miliseconds);

            return string.Format(CultureInfo.InvariantCulture, "{0:D2}:{1:D2}:{2:D2}.{3:D3}",t.Hours, t.Minutes, t.Seconds, t.Milliseconds);
        }
    }
}

using System.Text.RegularExpressions;

namespace AppMetricsMackerelReporter.Extensions
{
    public static class StringExtensions
    {
        public static string NoamalizeName(this string value)
        {
            var separated = SeparaterRegex.Replace(value, ".");
            return IrregularRegex.Replace(separated, "").ToLower().TrimStart('.').TrimEnd('.');
        }

        private static Regex SeparaterRegex = new Regex("[ \\.]");
        private static Regex IrregularRegex = new Regex("[^a-zA-Z0-9\\.]");
    }
}

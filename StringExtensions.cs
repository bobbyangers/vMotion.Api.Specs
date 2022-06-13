using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace vMotion.Api.Specs
{
    internal static class StringExtensions
    {
        private static Regex _objectIdRegEx = new Regex("[a-f0-9]{32}", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal static Guid ToGuid(this string subject)
        {
            if (subject?.Length < 32)
                subject = $"{new string('0', 32 - subject.Length)}{subject}";

            if (Guid.TryParse(subject, out var result)) return result;

            throw new ArgumentException($"Cannot parse [{subject}] to guid", nameof(subject));
        }

        internal static string ToObjectId(this Guid subject)
        {
            var data = subject.ToString("N").Substring(8, 24);
            return data;
        }

        internal static string ToObjectId(this string subject)
        {
            if (Guid.TryParse(subject, out Guid result))
            {
                return result.ToObjectId();
            }

            return vMotion.Dal.GuidHelper.NewUuid(subject).ToObjectId();
        }

        internal static string ObjectIdToGuidString(this string subject)
        {
            return subject.ToGuid().ToString("D");
        }

        internal static string ToJson(this object subject)
        {
            return JsonConvert.SerializeObject(subject, Formatting.Indented);
        }

        internal static string ToJson(this object subject, Formatting format)
        {
            return JsonConvert.SerializeObject(subject, format);
        }

        internal static string FormattedJson(this string subject)
        {
            var result = JsonConvert.DeserializeObject(subject.Replace(@"\\r\\n", Environment.NewLine)).ToJson();

            return result;
        }

        internal static T FromJson<T>(this object subject)
        {
            return JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(subject));
        }

        internal static T FromJson<T>(this string subject)
        {
            return JsonConvert.DeserializeObject<T>(subject);
        }

        internal static string SubstringLast(this string subject, int length)
        {
            return subject.Substring(subject.Length - length, length);
        }

        internal static bool MatchRegex(this string subject, string expression)
        {
            return MatchRegex(subject, new Regex(expression));
        }

        internal static bool MatchRegex(this string subject, Regex regex)
        {
            var result = regex.IsMatch(subject);

            return result;
        }
    }
}
using System;
using Orchard.Localization;
using Orchard.Mvc.Html;
using Orchard.Services;

namespace Orchard.Tokens.Providers {
    public class DateTokens : ITokenProvider {
        private readonly IClock _clock;

        public DateTokens(IClock clock) {
            _clock = clock;
            T = NullLocalizer.Instance;
        }

        public Localizer T { get; set; }

        public void Describe(DescribeContext context) {
            context.For("Date", T("Date"), T("Current Date tokens"))
                .Token("Since", T("Since"), T("Relative to the current date/time."))
                .Token("Format:*", T("Format:<date format>"), T("Optional format specifier (e.g. yyyy/MM/dd). See format strings at <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/az4se3k1.aspx\">Standard Formats</a> and <a target=\"_blank\" href=\"http://msdn.microsoft.com/en-us/library/8kb3ddd4.aspx\">Custom Formats</a>"), "DateTime");
        }

        public void Evaluate(EvaluateContext context) {
            context.For("Date", () => _clock.UtcNow)
                // {Date.Since}
                .Token("Since", DateTimeRelative)
                // {Date}
                .Token(
                    token => token == "" ? "" : null,
                    (token, d) => d.ToString())
                // {Date.Format:<formatstring>}
                .Token(
                    token => token.StartsWith("Format:", StringComparison.OrdinalIgnoreCase) ? token.Substring("Format:".Length) : null,
                    (token, d) => d.ToString(token));
        }

        private string DateTimeRelative(DateTime dateTimeUtc) {
            var time = _clock.UtcNow - dateTimeUtc.ToUniversalTime();

            if (time.TotalDays > 7)
                return dateTimeUtc.ToString(T("'on' MMM d yyyy 'at' h:mm tt").ToString());
            if (time.TotalHours > 24)
                return T.Plural("1 day ago", "{0} days ago", time.Days).ToString();
            if (time.TotalMinutes > 60)
                return T.Plural("1 hour ago", "{0} hours ago", time.Hours).ToString();
            if (time.TotalSeconds > 60)
                return T.Plural("1 minute ago", "{0} minutes ago", time.Minutes).ToString();
            if (time.TotalSeconds > 10)
                return T.Plural("1 second ago", "{0} seconds ago", time.Seconds).ToString();

            return T("a moment ago").ToString();
        }

    }
}
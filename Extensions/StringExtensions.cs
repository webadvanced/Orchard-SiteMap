using System;
using System.Linq;

namespace WebAdvanced.Sitemap.Extensions {
    public static class StringExtensions {
        public static string UrlToTitle(this string url) {
            var slug = url.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            return slug.SlugToTitle();
        }

        public static string SlugToTitle(this string slug) {
            var parts = slug.Split('-');
            return String.Join(" ", parts.Select(p => {
                char[] a = p.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            }).ToArray());
        }
    }
}

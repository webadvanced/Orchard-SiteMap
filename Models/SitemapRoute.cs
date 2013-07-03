using System;

namespace WebAdvanced.Sitemap.Models {
    public class SitemapRoute {
        public string Title { get; set; }
        public string Url { get; set; }

        /// <summary>
        /// The relative route that the url should be structured by in the sitemap tree.
        /// Providers can use this to explicitly structure a route tree that does not correspond to the actual urls.
        /// Very useful for, eg. routes that are absolute.
        /// </summary>
        public string UrlAlias { get; set; }

        /// <summary>
        /// Ignored for display
        /// </summary>
        public string UpdateFrequency { get; set; }

        /// <summary>
        /// Ignored for display
        /// </summary>
        public int Priority { get; set; }

        public DateTime? LastUpdated { get; set; }
    }
}

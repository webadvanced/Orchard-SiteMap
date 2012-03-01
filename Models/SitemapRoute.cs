using System;

namespace WebAdvanced.Sitemap.Models {
    public class SitemapRoute {
        public string Title { get; set; }
        public string Url { get; set; }

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

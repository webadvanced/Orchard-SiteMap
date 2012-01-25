using System.Collections.Generic;

namespace WebAdvanced.Sitemap.Models {
    public class SitemapNode {
        public SitemapNode(string title, string url = null) {
            Title = title;
            Url = url;
            Children = new Dictionary<string, SitemapNode>();
        }

        public string Title { get; set; }
        public string Url { get; set; }
        public Dictionary<string, SitemapNode> Children { get; set; }
    }
}

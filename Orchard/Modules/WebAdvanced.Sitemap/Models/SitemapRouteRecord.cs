namespace WebAdvanced.Sitemap.Models {
    public class SitemapRouteRecord {
        public virtual int Id { get; set; }
        public virtual string Slug { get; set; }
        public virtual int DisplayLevels { get; set; }
        public virtual bool Active { get; set; }
        public virtual int DisplayColumn { get; set; }
        public virtual int Weight { get; set; }
    }
}

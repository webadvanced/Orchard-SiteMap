namespace WebAdvanced.Sitemap.ViewModels {
    public class RouteSettingsModel {
        public int Id { get; set; }
        public string Slug { get; set; }
        public string Name { get; set; }
        public int DisplayLevels { get; set; }
        public bool Active { get; set; }
        public int DisplayColumn { get; set; }
        public int Weight { get; set; }
    }
}

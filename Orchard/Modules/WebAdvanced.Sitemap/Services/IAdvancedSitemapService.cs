using System.Collections.Generic;
using Orchard;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.ViewModels;

namespace WebAdvanced.Sitemap.Services {
    public interface IAdvancedSitemapService : IDependency {
        void SetIndexSettings(IEnumerable<IndexSettingsModel> settings);
        IEnumerable<IndexSettingsModel> GetIndexSettings();

        void SetRoutes(IEnumerable<RouteSettingsModel> settings);
        IEnumerable<RouteSettingsModel> GetRoutes();

        void SetCustomRoutes(IEnumerable<CustomRouteModel> settings);
        IEnumerable<CustomRouteModel> GetCustomRoutes();

        void DeleteCustomRoute(string url);

        SitemapNode GetSitemapRoot();
    }
}

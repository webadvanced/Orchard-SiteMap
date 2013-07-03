using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Themes;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.Providers;
using WebAdvanced.Sitemap.Services;
using WebAdvanced.Sitemap.ViewModels;

namespace WebAdvanced.Sitemap.Controllers {
    public class HomeController : Controller {
        private readonly IAdvancedSitemapService _sitemapService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ISitemapRouteFilter> _routeFilters;
        private readonly IEnumerable<ISitemapRouteProvider> _routeProviders;
        private readonly ISignals _signals;
        private readonly ISiteService _siteService;

        public dynamic Shape { get; set; }

        public HomeController(
            IAdvancedSitemapService sitemapService,
            IShapeFactory shapeFactory,
            ICacheManager cacheManager,
            IClock clock,
            IContentManager contentManager,
            IEnumerable<ISitemapRouteFilter> routeFilters,
            IEnumerable<ISitemapRouteProvider> routeProviders,
            ISignals signals,
            ISiteService siteService) {
            _sitemapService = sitemapService;
            _cacheManager = cacheManager;
            _clock = clock;
            _contentManager = contentManager;
            _routeFilters = routeFilters;
            _routeProviders = routeProviders;
            _signals = signals;
            _siteService = siteService;

            Shape = shapeFactory;
        }

        public ActionResult Xml() {
            var doc = _cacheManager.Get("sitemap.xml", ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromHours(1.0)));
                ctx.Monitor(_signals.When("WebAdvanced.Sitemap.XmlRefresh"));

                XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

                var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
                var urlset = new XElement(xmlns + "urlset");
                document.Add(urlset);

                var rootUrl = GetRootPath();

                // Add filtered routes
                var routeUrls = new HashSet<string>(); // Don't include the same url twice
                var items = new List<SitemapRoute>();

                // Process routes from providers in order of high to low priority to allow custom routes
                // to be processed first and thus override content routes.
                foreach (var provider in _routeProviders.OrderByDescending(p => p.Priority)) {
                    var validRoutes = provider.GetXmlRoutes()
                        .Where(r => _routeFilters.All(filter => filter.AllowUrl(r.Url)))
                        .AsEnumerable();

                    foreach (var item in validRoutes) {
                        if (routeUrls.Contains(item.Url))
                            continue;

                        routeUrls.Add(item.Url);
                        items.Add(item);
                    }
                }

                // Ensure routes with higher priority are listed first
                foreach (var item in items.OrderByDescending(i => i.Priority).ThenBy(i => i.Url)) {
                    string url = item.Url;
                    if (!Regex.IsMatch(item.Url, @"^\w+://.*$")) {
                        url = rootUrl + item.Url.TrimStart('/');
                    }
                    
                    var element = new XElement(xmlns + "url");
                    element.Add(new XElement(xmlns + "loc", url));
                    element.Add(new XElement(xmlns + "changefreq", item.UpdateFrequency));
                    if (item.LastUpdated.HasValue) {
                        element.Add(new XElement(xmlns + "lastmod", item.LastUpdated.Value.ToString("yyyy-MM-dd")));
                    }
                    var priority = (item.Priority - 1) / 4.0;
                    if (priority >= 0.0 && priority <= 1.0) {
                        element.Add(new XElement(xmlns + "priority", (item.Priority - 1) / 4.0));
                    }
                    urlset.Add(element);
                }
                
                return document;
            });

            return new XmlResult(doc);
        }

        [Themed]
        public ActionResult Index() {
            var root = _sitemapService.GetSitemapRoot();

            var routeSettings = _sitemapService.GetRoutes().Where(r => r.Active).ToList();

            var columnCount = routeSettings.Max(s => s.DisplayColumn);
            var columnShapes = new List<dynamic>();
            for (int i = 1; i <= columnCount; i++) {
                var routesInColumn = routeSettings
                    .Where(s => s.DisplayColumn == i)
                    .OrderBy(s => s.Weight)
                    .ToList();
                var groupShapes = routesInColumn
                    .Where(r => root.Children.ContainsKey(r.Slug))
                    .Select(r => BuildGroupShape(root.Children[r.Slug]));
                columnShapes.Add(Shape.Sitemap_Column(Groups: groupShapes));
            }

            return new ShapeResult(this, Shape.Sitemap_Index(
                Columns: columnShapes, 
                RouteSettings: routeSettings,
                ColumnCount: columnCount));
        }

        private dynamic BuildGroupShape(SitemapNode node) {
            var childShapes = node.Children.Values.Select(BuildNodeShape).ToList();
            return Shape.Sitemap_Group(Title: node.Title, Url: node.Url, Children: childShapes);
        }

        private dynamic BuildNodeShape(SitemapNode node) {
            var childShapes = node.Children.Values.Select(BuildNodeShape).ToList();
            return Shape.Sitemap_Node(Title: node.Title, Url: node.Url, Children: childShapes);
        }

        private string GetRootPath() {
            var baseUrl = _siteService.GetSiteSettings().BaseUrl;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";
            return baseUrl;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Core.Routable.Models;
using Orchard.DisplayManagement;
using Orchard.Mvc;
using Orchard.Services;
using Orchard.Themes;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.Services;
using WebAdvanced.Sitemap.ViewModels;

namespace WebAdvanced.Sitemap.Controllers {
    public class HomeController : Controller {
        private readonly IAdvancedSitemapService _sitemapService;
        private readonly ICacheManager _cacheManager;
        private readonly IClock _clock;
        private readonly IContentManager _contentManager;
        private readonly IEnumerable<ISitemapRouteFilter> _routeFilters;

        public dynamic Shape { get; set; }

        public HomeController(
            IAdvancedSitemapService sitemapService,
            IShapeFactory shapeFactory,
            ICacheManager cacheManager,
            IClock clock,
            IContentManager contentManager,
            IEnumerable<ISitemapRouteFilter> routeFilters) {
            _sitemapService = sitemapService;
            _cacheManager = cacheManager;
            _clock = clock;
            _contentManager = contentManager;
            _routeFilters = routeFilters;

            Shape = shapeFactory;
        }

        public ActionResult Xml() {
            var doc = _cacheManager.Get("sitemap.xml", ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromDays(1.0)));

                var ns = XNamespace.Get("http://www.sitemaps.org/schemas/sitemap/0.9");
                var document = new XDocument();
                var urlset = new XElement(ns + "urlset");
                document.Add(urlset);

                var rootUrl = GetRootPath();

                // Add custom routes
                var customRoutes = _sitemapService.GetCustomRoutes();
                foreach (var item in customRoutes) {
                    var url = rootUrl + item.Url.TrimStart('/');
                    var element = new XElement("url");
                    element.Add(new XElement("loc", url));
                    element.Add(new XElement("priority",
                        (item.Priority - 1) / 4.0).ToString());
                    element.Add(new XElement("changefreq", item.UpdateFrequency));
                    urlset.Add(element);
                }

                var routeSettings = new Dictionary<string, RouteSettingsModel>();
                var contentSettings = _sitemapService.GetIndexSettings()
                    .Where(s => s.IndexForXml)
                    .ToDictionary(
                        k => k.Name,
                        v => v);

                // Compile all content urls
                var contentItems = _contentManager.Query(VersionOptions.Published, contentSettings.Keys.ToArray()).List();
                foreach (var item in contentItems) {
                    if (!_routeFilters.All(filter => filter.AllowUrl(item.As<IRoutableAspect>().Path)))
                        continue;

                    var url = rootUrl + item.As<IRoutableAspect>().Path;
                    var dateModified = item.As<CommonPart>().ModifiedUtc;
                    var element = new XElement("url");
                    element.Add(new XElement("loc", url));
                    if (dateModified.HasValue)
                        element.Add(new XElement("lastmod", dateModified.Value.ToString("yyyy-MM-dd")));
                    element.Add(new XElement("priority", 
                        ((contentSettings[item.ContentType].Priority - 1) / 4.0).ToString()));
                    element.Add(new XElement("changefreq", contentSettings[item.ContentType].UpdateFrequency));
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
            //Getting the current context of HTTP request
            var context = HttpContext;

            //Checking the current context content
            if (context == null) return null;

            //Formatting the fully qualified website url/name
            var appPath = string.Format("{0}://{1}{2}{3}",
                                        context.Request.Url.Scheme,
                                        context.Request.Url.Host,
                                        context.Request.Url.Port == 80
                                            ? string.Empty
                                            : ":" + context.Request.Url.Port,
                                        context.Request.ApplicationPath);

            if (!appPath.EndsWith("/"))
                appPath += "/";
            return appPath;
        }

    }
}

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

        public dynamic Shape { get; set; }

        public HomeController(
            IAdvancedSitemapService sitemapService,
            IShapeFactory shapeFactory) {
            _sitemapService = sitemapService;

            Shape = shapeFactory;
        }

        public ActionResult Xml() {
            return new XmlResult(_sitemapService.GetSitemapDocument());
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
    }
}

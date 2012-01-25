using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using Orchard.Mvc.Routes;

namespace WebAdvanced.Sitemap {
    public class Routes : IRouteProvider {

        public void GetRoutes(ICollection<RouteDescriptor> routes) {
            foreach (var routeDescriptor in GetRoutes())
                routes.Add(routeDescriptor);
        }

        public IEnumerable<RouteDescriptor> GetRoutes() {
            return new[] {
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Sitemap/Indexing",
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"},
                            {"controller", "Admin"},
                            {"action", "Indexing"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Sitemap/Display",
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"},
                            {"controller", "Admin"},
                            {"action", "DisplaySettings"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "Admin/Sitemap/GetCustomRouteForm",
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"},
                            {"controller", "Admin"},
                            {"action", "GetNewCustomRouteForm"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "sitemap",
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"},
                            {"controller", "Home"},
                            {"action", "Index"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"}
                        },
                        new MvcRouteHandler())
                },
                new RouteDescriptor {
                    Route = new Route(
                        "sitemap.xml",
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"},
                            {"controller", "Home"},
                            {"action", "Xml"}
                        },
                        new RouteValueDictionary(),
                        new RouteValueDictionary {
                            {"area", "WebAdvanced.Sitemap"}
                        },
                        new MvcRouteHandler())
                }
            };
        }
    }
}

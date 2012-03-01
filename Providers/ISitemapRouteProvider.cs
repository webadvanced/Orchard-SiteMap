using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard;
using WebAdvanced.Sitemap.Models;

namespace WebAdvanced.Sitemap.Providers {
    public interface ISitemapRouteProvider : IDependency {
        IEnumerable<SitemapRoute> GetDisplayRoutes();
        IEnumerable<SitemapRoute> GetXmlRoutes();
    }
}

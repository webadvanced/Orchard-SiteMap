using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Autoroute.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.Core.Common.Models;
using Orchard.Data;
using WebAdvanced.Sitemap.Models;

namespace WebAdvanced.Sitemap.Providers.Impl {
    public class ContentRouteProvider : ISitemapRouteProvider {
        private readonly IRepository<SitemapSettingsRecord> _sitemapSettings;
        private readonly IContentManager _contentManager;

        public ContentRouteProvider(
            IRepository<SitemapSettingsRecord> sitemapSettings,
            IContentManager contentManager) {
            _sitemapSettings = sitemapSettings;
            _contentManager = contentManager;
        }

        public IEnumerable<SitemapRoute> GetDisplayRoutes() {
            // Get all active content types
            var types = _sitemapSettings
                .Fetch(q => q.IndexForDisplay)
                .ToDictionary(
                    k => k.ContentType,
                    v => v);

            if (types.Any()) {
                var contents = _contentManager.Query(VersionOptions.Published, types.Keys.ToArray()).List();

                return contents.Select(c => new SitemapRoute {
                    Priority = types[c.ContentType].Priority,
                    Title = _contentManager.GetItemMetadata(c).DisplayText,
                    UpdateFrequency = types[c.ContentType].UpdateFrequency,
                    Url = c.As<AutoroutePart>().Path
                }).AsEnumerable();
            }

            return new List<SitemapRoute>();
        }

        public IEnumerable<SitemapRoute> GetXmlRoutes() {
            // Get all active content types
            var types = _sitemapSettings
                .Fetch(q => q.IndexForXml)
                .ToDictionary(
                    k => k.ContentType,
                    v => v);

            if (types.Any()) {
                var contents = _contentManager.Query(VersionOptions.Published, types.Keys.ToArray()).List();

                return contents.Select(c => new SitemapRoute {
                    Priority = types[c.ContentType].Priority,
                    Title = _contentManager.GetItemMetadata(c).DisplayText,
                    UpdateFrequency = types[c.ContentType].UpdateFrequency,
                    Url = c.As<AutoroutePart>().Path,
                    LastUpdated = c.Has<CommonPart>() ? c.As<CommonPart>().ModifiedUtc : null
                }).AsEnumerable();
            }

            return new List<SitemapRoute>();
        }
    }
}

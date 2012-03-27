using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using WebAdvanced.Sitemap.Services;

namespace WebAdvanced.Sitemap.Handlers {
    public class SitemapContentHandler : ContentHandler {
        private readonly ISignals _signals;
        private readonly IAdvancedSitemapService _sitemapService;

        public SitemapContentHandler(
            ISignals signals,
            IAdvancedSitemapService sitemapService) {
            _signals = signals;
            _sitemapService = sitemapService;

            OnPublished<ContentItem>((ctx, item) => {
                var activeContentTypes = _sitemapService.GetIndexSettings()
                    .Where(m => m.IndexForDisplay || m.IndexForXml)
                    .Select(m => m.Name)
                    .ToList();
                if (activeContentTypes.Contains(ctx.ContentItem.ContentType)) {
                    _signals.Trigger("WebAdvanced.Sitemap.Refresh");
                }
            });
        }
    }
}

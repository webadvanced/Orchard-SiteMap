using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Data;
using Orchard.Services;
using Orchard.Settings;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.Providers;
using WebAdvanced.Sitemap.ViewModels;
using WebAdvanced.Sitemap.Extensions;

namespace WebAdvanced.Sitemap.Services {
    public class AdvancedSitemapService : IAdvancedSitemapService {
        readonly IRepository<SitemapRouteRecord> _routeRepository;
        readonly IRepository<SitemapSettingsRecord> _settingsRepository;
        private readonly IRepository<SitemapCustomRouteRecord> _customRouteRepository;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IClock _clock;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IEnumerable<ISitemapRouteFilter> _routeFilters;
        private readonly IEnumerable<ISitemapRouteProvider> _routeProviders;
        private readonly ISiteService _siteService;
        private readonly IEnumerable<ISpecializedSitemapProvider> _specializedSitemapProviders;

        public AdvancedSitemapService(
            IRepository<SitemapRouteRecord> routeRepository, 
            IRepository<SitemapSettingsRecord> settingsRepository,
            IRepository<SitemapCustomRouteRecord> customRouteRepository,
            IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals,
            IClock clock,
            IContentDefinitionManager contentDefinitionManager,
            IEnumerable<ISitemapRouteFilter> routeFilters,
            IEnumerable<ISitemapRouteProvider> routeProviders, 
            ISiteService siteService, 
            IEnumerable<ISpecializedSitemapProvider> specializedSitemapProviders) {
            _routeRepository = routeRepository;
            _settingsRepository = settingsRepository;
            _customRouteRepository = customRouteRepository;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
            _contentDefinitionManager = contentDefinitionManager;
            _routeFilters = routeFilters;
            _routeProviders = routeProviders;
            _siteService = siteService;
            _specializedSitemapProviders = specializedSitemapProviders;
        }

        public void SetIndexSettings(IEnumerable<IndexSettingsModel> settings) {
            foreach (var item in settings) {
                var item1 = item;
                var record = _settingsRepository.Fetch(q => q.ContentType == item1.Name).FirstOrDefault();
                if (record == null) {
                    record = new SitemapSettingsRecord();
                    record.ContentType = item.Name;
                    record.IndexForDisplay = item.IndexForDisplay;
                    record.IndexForXml = item.IndexForXml;
                    record.Priority = item.Priority;
                    record.UpdateFrequency = item.UpdateFrequency;
                    _settingsRepository.Create(record);
                } else {
                    record.IndexForDisplay = item.IndexForDisplay;
                    record.IndexForXml = item.IndexForXml;
                    record.Priority = item.Priority;
                    record.UpdateFrequency = item.UpdateFrequency;
                    _settingsRepository.Update(record);
                }
            }
            _signals.Trigger("WebAdvanced.Sitemap.IndexSettings");
        }

        public IEnumerable<IndexSettingsModel> GetIndexSettings() {
            var settings = _cacheManager.Get("WebAdvanced.Sitemap.IndexSettings", 
                ctx => {
                    ctx.Monitor(_signals.When("ContentDefinitionManager"));
                    ctx.Monitor(_signals.When("WebAdvanced.Sitemap.IndexSettings"));

                    var contentTypes = _contentDefinitionManager.ListTypeDefinitions()
                        .Where(ctd => ctd.Parts.FirstOrDefault(p => p.PartDefinition.Name == "AutoroutePart") != null)
                        .ToList();

                    var typeNames = contentTypes.Select(t => t.Name).ToArray();

                    // Delete everything that no longer corresponds to these allowed content types
                    var toDelete = _settingsRepository.Fetch(q => !typeNames.Contains(q.ContentType)).ToList();
                    foreach (var record in toDelete) {
                        _settingsRepository.Delete(record);
                    }
                    _settingsRepository.Flush();

                    var contentSettings = new List<IndexSettingsModel>();
                    foreach (var type in contentTypes) {
                        var _type = type;
                        // Get the record, generate a new one if it doesn't exist.
                        SitemapSettingsRecord record = _settingsRepository.Fetch(q => q.ContentType == _type.Name).FirstOrDefault();
                        if (record == null) {
                            record = new SitemapSettingsRecord();
                            record.ContentType = type.Name;
                            record.IndexForDisplay = false;
                            record.IndexForXml = false;
                            record.Priority = 3;
                            record.UpdateFrequency = "weekly";
                            _settingsRepository.Create(record);
                        }

                        var model = new IndexSettingsModel() {
                            DisplayName = type.DisplayName,
                            Name = type.Name,
                            IndexForDisplay = record.IndexForDisplay,
                            IndexForXml = record.IndexForXml,
                            Priority = record.Priority,
                            UpdateFrequency = record.UpdateFrequency
                        };
                        contentSettings.Add(model);
                    }

                    return contentSettings;
                });

            return settings;
        }

        public IEnumerable<CustomRouteModel> GetCustomRoutes() {
            return _cacheManager.Get("WebAdvanced.Sitemap.CustomRoutes", ctx => {
                ctx.Monitor(_signals.When("WebAdvanced.Sitemap.CustomRoutes"));

                var records = _customRouteRepository.Table.ToList();
                return records.Select(r => new CustomRouteModel {
                    IndexForDisplay = r.IndexForDisplay,
                    IndexForXml = r.IndexForXml,
                    Name = r.Url.Trim('/').Split('/')[0].SlugToTitle(),
                    Priority = r.Priority,
                    UpdateFrequency = r.UpdateFrequency ?? "weekly",
                    Url = r.Url ?? String.Empty
                });
            });
        }

        public void SetCustomRoutes(IEnumerable<CustomRouteModel> routes) {
            var existingRouteIds = new List<int>();

            foreach (var model in routes) {
                // Treat empty url as over-ride for root path
                if (string.IsNullOrWhiteSpace(model.Url)) {
                    model.Url = string.Empty;
                }

                var customRouteModel = model;
                var record = _customRouteRepository.Fetch(q => q.Url == customRouteModel.Url).FirstOrDefault();
                if (record == null) {
                    record = new SitemapCustomRouteRecord {
                        IndexForDisplay = model.IndexForDisplay,
                        IndexForXml = model.IndexForXml,
                        Priority = model.Priority,
                        Url = model.Url ?? String.Empty,
                        UpdateFrequency = model.UpdateFrequency
                    };
                    _customRouteRepository.Create(record);
                } else {
                    record.IndexForDisplay = model.IndexForDisplay;
                    record.IndexForXml = model.IndexForXml;
                    record.Priority = model.Priority;
                    record.UpdateFrequency = model.UpdateFrequency;
                    _customRouteRepository.Update(record);
                }
                existingRouteIds.Add(record.Id);
            }
            _customRouteRepository.Flush();

            var toDelete = _customRouteRepository.Fetch(q => !existingRouteIds.Contains(q.Id));
            foreach (var record in toDelete) {
                _customRouteRepository.Delete(record);
            }
            _customRouteRepository.Flush();

            _signals.Trigger("WebAdvanced.Sitemap.CustomRoutes");
        }

        public void DeleteCustomRoute(string url) {
            var record = _customRouteRepository.Fetch(q => q.Url == url).FirstOrDefault();
            if (record != null) {
                _customRouteRepository.Delete(record);
                _customRouteRepository.Flush();
                _signals.Trigger("WebAdvanced.Sitemap.CustomRoutes");
            }
        }

        private IEnumerable<string> GetActiveDisplayContentTypes() {
            return GetIndexSettings().Where(q => q.IndexForDisplay).Select(q => q.Name).ToList();
        }

        public IEnumerable<RouteSettingsModel> GetRoutes() {
            return _cacheManager.Get("WebAdvanced.Sitemap.Routes",
                ctx => {
                    ctx.Monitor(_signals.When("WebAdvanced.Sitemap.Refresh"));
                    var slugs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); // slug => Title (if available)

                    // Extract filtered routes from route providers
                    foreach(var provider in _routeProviders) {
                        var routes = provider.GetDisplayRoutes()
                            .Where(route => _routeFilters.All(filter => filter.AllowUrl(route.Url)))
                            .AsEnumerable();

                        // Get all base paths
                        foreach (var route in routes) {
                            var alias = route.UrlAlias ?? route.Url;
                            var slugParts = alias.Trim('/').Split('/');
                            if (slugParts.Count() == 1 && !string.IsNullOrWhiteSpace(route.Title)) {
                                slugs[slugParts[0]] = route.Title;
                            }
                            else {
                                if (!slugs.ContainsKey(slugParts[0]))
                                    slugs[slugParts[0]] = slugParts[0].SlugToTitle();
                            }
                        }
                    }

                    var routeModels = new List<RouteSettingsModel>();

                    var orderedSlugs = slugs.OrderByDescending(s => s.Value).ToList();
                    foreach (var pair in orderedSlugs) {
                        var slug = pair.Key;
                        var route = _routeRepository.Fetch(q => q.Slug == slug).FirstOrDefault();
                        if (route == null) {
                            route = new SitemapRouteRecord {
                                Active = true,
                                DisplayColumn = 1,
                                DisplayLevels = 3,
                                Weight = 0,
                                Slug = slug
                            };

                            _routeRepository.Create(route);
                        }
                        var model = new RouteSettingsModel {
                            Active = route.Active,
                            DisplayColumn = route.DisplayColumn,
                            DisplayLevels = route.DisplayLevels,
                            Id = route.Id,
                            Name = pair.Value,
                            Weight = route.Weight,
                            Slug = slug
                        };
                        routeModels.Add(model);
                    }

                    return routeModels;
                });
        }

        public void SetRoutes(IEnumerable<RouteSettingsModel> routes) {
            foreach(var item in routes) {
                var record = _routeRepository.Fetch(q => q.Id == item.Id).FirstOrDefault();
                if (record != null) {
                    record.Active = item.Active;
                    record.DisplayColumn = item.DisplayColumn;
                    record.DisplayLevels = item.DisplayLevels;
                    record.Weight = item.Weight;
                    _routeRepository.Update(record);
                }
            }
            _signals.Trigger("WebAdvanced.Sitemap.Refresh");
            _routeRepository.Flush();
        }

        public SitemapNode GetSitemapRoot() {
            // Create dictionary indexed by routes
            var routeSettings = GetRoutes().ToDictionary(
                k => k.Slug,
                v => v, 
                StringComparer.OrdinalIgnoreCase);

            var sitemapRoot = _cacheManager.Get("WebAdvanced.Sitemap.Root", ctx => {
                ctx.Monitor(_signals.When("WebAdvanced.Sitemap.Refresh"));
                
                var sitemap = new SitemapNode("Root", null);

                foreach (var provider in _routeProviders.OrderByDescending(p => p.Priority)) {
                    var validRoutes = provider.GetDisplayRoutes()
                        .Where(r => _routeFilters.All(filter => filter.AllowUrl(r.Url)))
                        .AsEnumerable();

                    foreach (var item in validRoutes) {
                        var alias = item.UrlAlias ?? item.Url;
                        var slugs = alias.Split('/').ToArray();
                        var routeSetting = routeSettings.ContainsKey(slugs[0]) ? routeSettings[slugs[0]] : null;

                        // Only add this to the sitemap if the route settings exist and accept it
                        if (routeSetting == null || !routeSetting.Active || slugs.Count() > routeSetting.DisplayLevels)
                            continue;

                        int i = 0;
                        SitemapNode currentNode = sitemap;
                        while (i < slugs.Length) {
                            var isLeaf = i == slugs.Length - 1;
                            if (!currentNode.Children.ContainsKey(slugs[i])) {
                                string name = isLeaf ? item.Title : slugs[i].SlugToTitle();
                                string url = isLeaf ? item.Url : null;
                                currentNode.Children.Add(slugs[i], new SitemapNode(name, url));
                            } 
                            else if (isLeaf) { // Only replace existing items if the current is a leaf 
                                currentNode.Children[slugs[i]].Url = item.Url;
                                // Keep current title if the over-riding one is empty  when a custom route is over-riding a content route
                                if (!string.IsNullOrWhiteSpace(item.Title)) {
                                    currentNode.Children[slugs[i]].Title = item.Title; 
                                }
                            }
                            currentNode = currentNode.Children[slugs[i]];
                            i++;
                        }
                    }
                }

                return sitemap;
            });

            return sitemapRoot;
        }

        private string GetRootPath()
        {
            var baseUrl = _siteService.GetSiteSettings().BaseUrl;
            if (!baseUrl.EndsWith("/"))
                baseUrl += "/";
            return baseUrl;
        }

        public XDocument GetSitemapDocument()
        {
            return _cacheManager.Get("sitemap.xml", BuildSitemapDocument);
        }

        private XDocument BuildSitemapDocument(AcquireContext<string> ctx) {
            ctx.Monitor(_clock.When(TimeSpan.FromHours(1.0)));
            ctx.Monitor(_signals.When("WebAdvanced.Sitemap.XmlRefresh"));

            XNamespace xmlns = "http://www.sitemaps.org/schemas/sitemap/0.9";

            var providerContext = new DescribeSpecializedSitemapProviderContext();
            foreach (var specializedSitemapProvider in _specializedSitemapProviders) {
                specializedSitemapProvider.Describe(providerContext);
            }

            var document = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            var urlset = new XElement(xmlns + "urlset");

            foreach (var specializedSitemapFor in providerContext.Describes.Values) {
                urlset.Add(new XAttribute(XNamespace.Xmlns + specializedSitemapFor.NamespacePrefix, specializedSitemapFor.XNamespace));
            }

            document.Add(urlset);

            var rootUrl = GetRootPath();

            // Add filtered routes
            var routeUrls = new HashSet<string>(); // Don't include the same url twice
            var items = new List<SitemapRoute>();

            // Process routes from providers in order of high to low priority to allow custom routes
            // to be processed first and thus override content routes.
            foreach (var provider in _routeProviders.OrderByDescending(p => p.Priority)) {
                var validRoutes = provider.GetXmlRoutes().Where(r => _routeFilters.All(filter => filter.AllowUrl(r.Url))).AsEnumerable();

                foreach (var item in validRoutes) {
                    if (routeUrls.Contains(item.Url)) {
                        continue;
                    }

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
                var priority = (item.Priority - 1)/4.0;
                if (priority >= 0.0 && priority <= 1.0) {
                    element.Add(new XElement(xmlns + "priority", (item.Priority - 1)/4.0));
                }

                foreach (var specializedSitemapFor in providerContext.Describes.Values) {
                    var xElement = specializedSitemapFor.Process(item.ContentItem, item.Url);
                    if (xElement != null) {
                        element.Add(xElement);
                    }
                }

                urlset.Add(element);
            }

            return document;
        }
    }
}

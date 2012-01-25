using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchard.Caching;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Aspects;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.Core.Routable.Models;
using Orchard.Data;
using Orchard.Services;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.ViewModels;
using Orchard.Core.Routable.Services;

namespace WebAdvanced.Sitemap.Services {
    [UsedImplicitly]
    public class AdvancedSitemapService : IAdvancedSitemapService {
        readonly IRepository<SitemapRouteRecord> _routeRepository;
        readonly IRepository<SitemapSettingsRecord> _settingsRepository;
        private readonly IRepository<SitemapCustomRouteRecord> _customRouteRepository;
        private readonly IContentManager _contentManager;
        private readonly ICacheManager _cacheManager;
        private readonly ISignals _signals;
        private readonly IClock _clock;
        private readonly IContentDefinitionManager _contentDefinitionManager;

        public AdvancedSitemapService(
            IRepository<SitemapRouteRecord> routeRepository, 
            IRepository<SitemapSettingsRecord> settingsRepository,
            IRepository<SitemapCustomRouteRecord> customRouteRepository,
            IContentManager contentManager,
            ICacheManager cacheManager,
            ISignals signals,
            IClock clock,
            IContentDefinitionManager contentDefinitionManager) {
            _routeRepository = routeRepository;
            _settingsRepository = settingsRepository;
            _customRouteRepository = customRouteRepository;
            _contentManager = contentManager;
            _cacheManager = cacheManager;
            _signals = signals;
            _clock = clock;
            _contentDefinitionManager = contentDefinitionManager;
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
                        .Where(ctd => 
                            ctd.Settings.GetModel<ContentTypeSettings>().Creatable &&
                            ctd.Parts.Where(p => p.PartDefinition.Name == "RoutePart").FirstOrDefault() != null)
                        .ToList();

                    // Delete everything that no longer corresponds to these allowed content types
                    var toDelete = _settingsRepository.Fetch(q => !contentTypes.Exists(p => p.Name == q.ContentType)).ToList();
                    foreach (var record in toDelete) {
                        _settingsRepository.Delete(record);
                    }
                    _settingsRepository.Flush();

                    var contentSettings = new List<IndexSettingsModel>();
                    foreach (var type in contentTypes) {
                        // Get the record, generate a new one if it doesn't exist.
                        SitemapSettingsRecord record = _settingsRepository.Fetch(q => q.ContentType == type.Name).FirstOrDefault();
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

                    // TODO: Get custom route settings

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
                    Name = SlugToName(r.Url.Trim('/').Split('/')[0]),
                    Priority = r.Priority,
                    UpdateFrequency = r.UpdateFrequency ?? "weekly",
                    Url = r.Url ?? String.Empty
                });
            });
        }

        public void SetCustomRoutes(IEnumerable<CustomRouteModel> routes) {
            var existingRouteIds = new List<int>();

            foreach (var model in routes) {
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
    

        private IEnumerable<string> GetActiveXmlContentTypes() {
            return _settingsRepository.Fetch(q => q.IndexForXml).Select(s => s.ContentType).ToList();
        }

        private IEnumerable<string> GetActiveDisplayContentTypes() {
            return GetIndexSettings().Where(q => q.IndexForDisplay).Select(q => q.Name).ToList();
        }

        public IEnumerable<RouteSettingsModel> GetRoutes() {
            return _cacheManager.Get("WebAdvanced.Sitemap.Routes",
                ctx => {
                    ctx.Monitor(_signals.When("WebAdvanced.Sitemap.Refresh"));
                    var slugs = new Dictionary<string, string>(); // slug => Title (if available)
                    
                    // Get all active content types
                    var types = GetActiveDisplayContentTypes();
                    if (types.Any()) {
                        var contents = _contentManager.Query(VersionOptions.Published, types.ToArray()).List();

                        // Get all base paths

                        foreach (var item in contents) {
                            var routable = item.As<IRoutableAspect>();
                            var slugParts = routable.Path.Trim('/').Split('/');
                            if (slugParts.Count() == 1) {
                                slugs[slugParts[0]] = item.As<IRoutableAspect>().Title;
                            }
                            else {
                                if (!slugs.ContainsKey(slugParts[0]))
                                    slugs[slugParts[0]] = SlugToName(slugParts[0]);
                            }
                        }
                    }

                    // Add custom paths
                    var customRoutes = GetCustomRoutes();
                    foreach (var item in customRoutes) {
                        var slugParts = item.Url.Trim('/').Split('/');
                        slugs[slugParts[0]] = item.Name;
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

                    //_routeRepository.Flush();
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
            var activeTypes = GetActiveDisplayContentTypes();

            // Create dictionary indexed by routes
            var routes = GetRoutes().ToDictionary(
                k => k.Slug,
                v => v);

            var sitemapRoot = _cacheManager.Get("WebAdvanced.Sitemap.Root", ctx => {
                ctx.Monitor(_clock.When(TimeSpan.FromHours(1.0)));
                ctx.Monitor(_signals.When("WebAdvanced.Sitemap.Refresh"));

                var contents = _contentManager.Query(VersionOptions.Published, activeTypes.ToArray()).List();

                var sitemap = new SitemapNode("Root", null);

                foreach (var item in contents) {
                    var slugs = item.As<IRoutableAspect>().Path.Split('/').ToArray();
                    var route = routes.ContainsKey(slugs[0]) ? routes[slugs[0]] : null;

                    // Only add this to the sitemap if the route settings exist and accept it
                    if (route == null || !route.Active || slugs.Count() > route.DisplayLevels)
                        continue;

                    int i = 0;
                    SitemapNode currentNode = sitemap;
                    while (i < slugs.Length) {
                        if (!currentNode.Children.ContainsKey(slugs[i])) {
                            var isLeaf = i == slugs.Length - 1;
                            string name = isLeaf ? item.As<RoutePart>().Title : SlugToName(slugs[i]);
                            string url = isLeaf ? item.As<RoutePart>().Slug : null;
                            currentNode.Children.Add(slugs[i], new SitemapNode(name, url));
                        }
                        currentNode = currentNode.Children[slugs[i]];
                        i++;
                    }
                }

                var customRoutes = GetCustomRoutes();
                foreach (var item in customRoutes) {
                    var slugs = item.Url.Split('/').ToArray();
                    var route = routes.ContainsKey(slugs[0]) ? routes[slugs[0]] : null;

                    // Only add this to the sitemap if the route settings exist and accept it
                    if (route == null || !route.Active || slugs.Count() > route.DisplayLevels)
                        continue;

                    int i = 0;
                    SitemapNode currentNode = sitemap;
                    while (i < slugs.Length) {
                        if (!currentNode.Children.ContainsKey(slugs[i])) {
                            var isLeaf = i == slugs.Length - 1;
                            string name = SlugToName(slugs[i]);
                            string url = isLeaf ? item.Url : null;
                            currentNode.Children.Add(slugs[i], new SitemapNode(name, url));
                        }
                        currentNode = currentNode.Children[slugs[i]];
                        i++;
                    }
                }
                return sitemap;
            });

            return sitemapRoot;
        }

        private static string SlugToName(string slug) {
            var parts = slug.Split('-');
            return String.Join(" ", parts.Select(p => {
                char[] a = p.ToCharArray();
                a[0] = char.ToUpper(a[0]);
                return new string(a);
            }).ToArray());
        }
    }
}

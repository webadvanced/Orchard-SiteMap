using System;
using System.Collections.Generic;
using Orchard.Data;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using WebAdvanced.Sitemap.Models;
using System.Xml.Linq;

namespace WebAdvanced.Sitemap.Recipes.Executors
{
    public class SitemapStep : RecipeExecutionStep
    {
        private readonly IRepository<SitemapRouteRecord> _routeRepository;
        private readonly IRepository<SitemapSettingsRecord> _settingsRepository;
        private readonly IRepository<SitemapCustomRouteRecord> _customRouteRepository;

        public SitemapStep(
            IRepository<SitemapRouteRecord> routeRepository,
            IRepository<SitemapSettingsRecord> settingsRepository,
            IRepository<SitemapCustomRouteRecord> customRouteRepository,
            RecipeExecutionLogger logger) : base(logger)
        {
            _routeRepository = routeRepository;
            _settingsRepository = settingsRepository;
            _customRouteRepository = customRouteRepository;
        }

        public override string Name {
            get { return "WebAdvancedSitemap"; }
        }

        public override void Execute(RecipeExecutionContext context)
        {
            ProcessRoutes(context.RecipeStep.Step);
            ProcessSettings(context.RecipeStep.Step);
            ProcessCustomRoutes(context.RecipeStep.Step);
        }

        private void ProcessRoutes(XElement root)
        {
            var routeDefinitionsElement = root.Element("Routes");
            if (routeDefinitionsElement == null)
            {
                return;
            }

            foreach (var routeDefinitionElement in routeDefinitionsElement.Elements())
            {
                var routeSlug = routeDefinitionElement.Attribute("Slug").Value;
                Logger.Information("Importing route '{0}'.", routeSlug);

                try
                {
                    var routeDefinition = GetOrCreateRouteDefinition(routeSlug);
                    routeDefinition.DisplayLevels = int.Parse(routeDefinitionElement.Attribute("DisplayLevels").Value);
                    routeDefinition.Active = bool.Parse(routeDefinitionElement.Attribute("Active").Value);
                    routeDefinition.DisplayColumn = int.Parse(routeDefinitionElement.Attribute("DisplayColumn").Value);
                    routeDefinition.Weight = int.Parse(routeDefinitionElement.Attribute("Weight").Value);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while importing route '{0}'.", routeSlug);
                    throw;
                }
            }
        }

        private SitemapRouteRecord GetOrCreateRouteDefinition(string slug)
        {
            var routeDefinition = _routeRepository.Get(x => x.Slug == slug);

            if(routeDefinition == null)
            {
                routeDefinition = new SitemapRouteRecord
                {
                    Slug = slug
                };
                _routeRepository.Create(routeDefinition);
            }

            return routeDefinition;
        }

        private void ProcessSettings(XElement root)
        {
            var settingsDefinitionsElement = root.Element("Settings");
            if (settingsDefinitionsElement == null)
            {
                return;
            }

            foreach (var settingDefinitionElement in settingsDefinitionsElement.Elements())
            {
                var settingContentType = settingDefinitionElement.Attribute("ContentType").Value;
                Logger.Information("Importing settings '{0}'.", settingContentType);

                try
                {
                    var settingDefinition = GetOrCreateSettingDefinition(settingContentType);
                    settingDefinition.IndexForDisplay = bool.Parse(settingDefinitionElement.Attribute("IndexForDisplay").Value);
                    settingDefinition.IndexForXml = bool.Parse(settingDefinitionElement.Attribute("IndexForXml").Value);
                    settingDefinition.UpdateFrequency = settingDefinitionElement.Attribute("UpdateFrequency").Value;
                    settingDefinition.Priority = int.Parse(settingDefinitionElement.Attribute("Priority").Value);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while importing setting '{0}'.", settingContentType);
                    throw;
                }
            }
        }

        private SitemapSettingsRecord GetOrCreateSettingDefinition(string contentType)
        {
            var settingDefinition = _settingsRepository.Get(x => x.ContentType == contentType);

            if (settingDefinition == null)
            {
                settingDefinition = new SitemapSettingsRecord
                {
                    ContentType = contentType
                };
                _settingsRepository.Create(settingDefinition);
            }

            return settingDefinition;
        }

        private void ProcessCustomRoutes(XElement root)
        {
            var customRoutesDefinitionsElement = root.Element("CustomRoutes");
            if (customRoutesDefinitionsElement == null)
            {
                return;
            }

            foreach (var customRouteDefinitionElement in customRoutesDefinitionsElement.Elements())
            {
                var customRouteUrl = customRouteDefinitionElement.Attribute("Url").Value;
                Logger.Information("Importing custom route '{0}'.", customRouteUrl);

                try
                {
                    var customRouteDefinition = GetOrCreateCustomRouteDefinition(customRouteUrl);
                    customRouteDefinition.IndexForDisplay = bool.Parse(customRouteDefinitionElement.Attribute("IndexForDisplay").Value);
                    customRouteDefinition.IndexForXml = bool.Parse(customRouteDefinitionElement.Attribute("IndexForXml").Value);
                    customRouteDefinition.UpdateFrequency = customRouteDefinitionElement.Attribute("UpdateFrequency").Value;
                    customRouteDefinition.Priority = int.Parse(customRouteDefinitionElement.Attribute("Priority").Value);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Error while importing custom route '{0}'.", customRouteUrl);
                    throw;
                }
            }
        }

        private SitemapCustomRouteRecord GetOrCreateCustomRouteDefinition(string url)
        {
            var customRouteDefinition = _customRouteRepository.Get(x => x.Url == url);

            if (customRouteDefinition == null)
            {
                customRouteDefinition = new SitemapCustomRouteRecord
                {
                    Url = url
                };
                _customRouteRepository.Create(customRouteDefinition);
            }

            return customRouteDefinition;
        }
    }
}

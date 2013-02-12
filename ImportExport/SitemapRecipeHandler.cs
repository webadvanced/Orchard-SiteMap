using System;
using System.Linq;
using System.Xml.Linq;
using Orchard.Logging;
using Orchard.Recipes.Models;
using Orchard.Recipes.Services;
using WebAdvanced.Sitemap.Services;
using WebAdvanced.Sitemap.ViewModels;

namespace WebAdvanced.Sitemap.ImportExport
{
    public class SitemapRecipeHandler : IRecipeHandler
    {
        private readonly IAdvancedSitemapService _sitemapService;

        public SitemapRecipeHandler(IAdvancedSitemapService sitemapService)
        {
            _sitemapService = sitemapService;
            Logger = NullLogger.Instance;
        }

        public ILogger Logger { get; set; }

        public void ExecuteRecipeStep(RecipeContext recipeContext)
        {
            if (!String.Equals(recipeContext.RecipeStep.Name, SitemapCustomExportStep.ExportStep, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var stepElement = recipeContext.RecipeStep.Step;

            var indexingElement = stepElement.Element("Indexing");
            if (indexingElement != null)
            {
                var indexSettingsModels = indexingElement.Elements("Index").Select(element => new IndexSettingsModel
                {
                    Name = element.Attribute("Name").Value,
                    DisplayName = element.Attribute("DisplayName").Value,
                    IndexForDisplay = Convert.ToBoolean(element.Attribute("IndexForDisplay").Value),
                    IndexForXml = Convert.ToBoolean(element.Attribute("IndexForXml").Value),
                    Priority = Convert.ToInt32(element.Attribute("Priority").Value),
                    UpdateFrequency = element.Attribute("UpdateFrequency").Value
                });

                _sitemapService.SetIndexSettings(indexSettingsModels);
            }

            var customRoutesElement = stepElement.Element("CustomRoutes");
            if (customRoutesElement != null)
            {
                var customRouteModels = customRoutesElement.Elements("CustomRoute").Select(element => new CustomRouteModel
                {
                    Name = element.Attribute("Name").Value,
                    IndexForDisplay = Convert.ToBoolean(element.Attribute("IndexForDisplay").Value),
                    IndexForXml = Convert.ToBoolean(element.Attribute("IndexForXml").Value),
                    Priority = Convert.ToInt32(element.Attribute("Priority").Value),
                    UpdateFrequency = element.Attribute("UpdateFrequency").Value,
                    Url = element.Attribute("Url").Value
                });

                _sitemapService.SetCustomRoutes(customRouteModels);
            }

            var displayRoutesElement = stepElement.Element("DisplayRoutes");
            if (displayRoutesElement != null)
            {
                var routeSettingsModels = displayRoutesElement.Elements("DisplayRoute").Select(element => new RouteSettingsModel
                {
                    Name = element.Attribute("Name").Value,
                    Id = Convert.ToInt32(element.Attribute("Id").Value),
                    Active = Convert.ToBoolean(element.Attribute("Active").Value),
                    DisplayColumn = Convert.ToInt32(element.Attribute("DisplayColumn").Value),
                    DisplayLevels = Convert.ToInt32(element.Attribute("DisplayLevels").Value),
                    Slug = element.Attribute("Slug").Value,
                    Weight = Convert.ToInt32(element.Attribute("Weight").Value),
                });

                _sitemapService.SetRoutes(routeSettingsModels);
            }

            recipeContext.Executed = true;
        }
    }
}

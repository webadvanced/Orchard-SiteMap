using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Orchard.Events;
using WebAdvanced.Sitemap.Services;

namespace WebAdvanced.Sitemap.ImportExport
{
    public interface IExportEventHandler : IEventHandler
    {
        void Exporting(dynamic context);
        void Exported(dynamic context);
    }

    public class SitemapExportHandler : IExportEventHandler
    {
        private readonly IAdvancedSitemapService _sitemapService;

        public SitemapExportHandler(IAdvancedSitemapService sitemapService)
        {
            _sitemapService = sitemapService;
        }

        public void Exporting(dynamic context)
        {
        }

        public void Exported(dynamic context)
        {
            if (!((IEnumerable<string>)context.ExportOptions.CustomSteps).Contains(SitemapCustomExportStep.ExportStep))
            {
                return;
            }

            var indexing = new XElement("Indexing");
            var customRoutes = new XElement("CustomRoutes");
            var displayRoutes = new XElement("DisplayRoutes");

            var sitemap = new XElement(SitemapCustomExportStep.ExportStep, indexing, customRoutes, displayRoutes);
            context.Document.Element("Orchard").Add(sitemap);

            var indexSettingsModels = _sitemapService.GetIndexSettings();
            foreach (var indexSettingsModel in indexSettingsModels)
            {
                var index = new XElement("Index");
                indexing.Add(index);

                index.Add(new XAttribute("Name", indexSettingsModel.Name));
                index.Add(new XAttribute("DisplayName", indexSettingsModel.DisplayName));
                index.Add(new XAttribute("IndexForDisplay", indexSettingsModel.IndexForDisplay));
                index.Add(new XAttribute("IndexForXml", indexSettingsModel.IndexForXml));
                index.Add(new XAttribute("Priority", indexSettingsModel.Priority));
                index.Add(new XAttribute("UpdateFrequency", indexSettingsModel.UpdateFrequency));
            }

            var customRouteModels = _sitemapService.GetCustomRoutes();
            foreach (var customRouteModel in customRouteModels)
            {
                var customRoute = new XElement("CustomRoute");
                customRoutes.Add(customRoute);

                customRoute.Add(new XAttribute("Name", customRouteModel.Name));
                customRoute.Add(new XAttribute("IndexForDisplay", customRouteModel.IndexForDisplay));
                customRoute.Add(new XAttribute("IndexForXml", customRouteModel.IndexForXml));
                customRoute.Add(new XAttribute("Priority", customRouteModel.Priority));
                customRoute.Add(new XAttribute("UpdateFrequency", customRouteModel.UpdateFrequency));
                customRoute.Add(new XAttribute("Url", customRouteModel.Url));
            }

            var routeSettingsModels = _sitemapService.GetRoutes();
            foreach (var routeSettingsModel in routeSettingsModels) {
                var displayRoute = new XElement("DisplayRoute");
                displayRoutes.Add(displayRoute);

                displayRoute.Add(new XAttribute("Id", routeSettingsModel.Id));
                displayRoute.Add(new XAttribute("Name", routeSettingsModel.Name));
                displayRoute.Add(new XAttribute("Active", routeSettingsModel.Active));
                displayRoute.Add(new XAttribute("DisplayColumn", routeSettingsModel.DisplayColumn));
                displayRoute.Add(new XAttribute("DisplayLevels", routeSettingsModel.DisplayLevels));
                displayRoute.Add(new XAttribute("Slug", routeSettingsModel.Slug));
                displayRoute.Add(new XAttribute("Weight", routeSettingsModel.Weight));
            }
        }
    }
}
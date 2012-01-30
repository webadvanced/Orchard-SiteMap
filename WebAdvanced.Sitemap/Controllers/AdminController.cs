using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using Orchard;
using Orchard.ContentManagement;
using Orchard.ContentManagement.MetaData;
using Orchard.Core.Contents.Settings;
using Orchard.DisplayManagement;
using Orchard.Localization;
using Orchard.Mvc;
using Orchard.UI.Notify;
using WebAdvanced.Sitemap.Models;
using WebAdvanced.Sitemap.Services;
using WebAdvanced.Sitemap.ViewModels;

namespace WebAdvanced.Sitemap.Controllers {
    public class AdminController : Controller {
        private readonly IAdvancedSitemapService _sitemapService;
        private readonly IOrchardServices _services;

        public dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public AdminController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IAdvancedSitemapService sitemapService,
            IShapeFactory shapeFactory,
            INotifier notifier,
            IOrchardServices services) {
            _sitemapService = sitemapService;
            _services = services;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public ActionResult Indexing() {
            if (!_services.Authorizer.Authorize(Permissions.ManageSitemap, T("Not allowed to manage sitemap")))
                return new HttpUnauthorizedResult();

            var typeSettings = _sitemapService.GetIndexSettings();
            var customRoutes = _sitemapService.GetCustomRoutes();

            var model = new IndexingPageModel {
                ContentTypeSettings = typeSettings.OrderBy(q => q.DisplayName).ToList(),
                CustomRoutes = customRoutes.ToList()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Indexing(IndexingPageModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageSitemap, T("Not allowed to manage sitemap")))
                return new HttpUnauthorizedResult();
            
            if (model.CustomRoutes == null) {
                model.CustomRoutes = new List<CustomRouteModel>();
            }
            
            _sitemapService.SetIndexSettings(model.ContentTypeSettings);
            _sitemapService.SetCustomRoutes(model.CustomRoutes);

            _services.Notifier.Add(NotifyType.Information, T("Saved Sitemap indexing settings"));
            return RedirectToAction("Indexing");
        }

        public ActionResult DisplaySettings() {
            if (!_services.Authorizer.Authorize(Permissions.ManageSitemap, T("Not allowed to manage sitemap")))
                return new HttpUnauthorizedResult();

            var routes = _sitemapService.GetRoutes();
            var model = new DisplaySettingsPageModel {
                AutoLayout = false,
                Routes = routes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult DisplaySettings(DisplaySettingsPageModel model) {
            if (!_services.Authorizer.Authorize(Permissions.ManageSitemap, T("Not allowed to manage sitemap")))
                return new HttpUnauthorizedResult();

            _sitemapService.SetRoutes(model.Routes);
            _services.Notifier.Add(NotifyType.Information, T("Saved Sitemap display layout"));
            return RedirectToAction("DisplaySettings");
        }

        public ActionResult GetNewCustomRouteForm() {
            var emptyModel = new CustomRouteModel {
                IndexForDisplay = false,
                IndexForXml = false,
                Name = string.Empty,
                Priority = 3,
                UpdateFrequency = "weekly",
                Url = string.Empty
            };
            return PartialView("PartialCustomRouteEditor", emptyModel);
        }
    }
}

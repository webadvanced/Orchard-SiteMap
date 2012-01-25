using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
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
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly IAdvancedSitemapService _sitemapService;
        private readonly INotifier _notifier;

        public dynamic Shape { get; set; }

        public Localizer T { get; set; }

        public AdminController(
            IContentManager contentManager,
            IContentDefinitionManager contentDefinitionManager,
            IAdvancedSitemapService sitemapService,
            IShapeFactory shapeFactory,
            INotifier notifier) {
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _sitemapService = sitemapService;
            _notifier = notifier;
            Shape = shapeFactory;
            T = NullLocalizer.Instance;
        }

        public ActionResult Indexing() {
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
            _sitemapService.SetIndexSettings(model.ContentTypeSettings);
            if (model.CustomRoutes != null) {
                _sitemapService.SetCustomRoutes(model.CustomRoutes);
            }

            _notifier.Add(NotifyType.Information, T("Saved Sitemap indexing settings"));
            return RedirectToAction("Indexing");
        }

        public ActionResult DisplaySettings() {
            var routes = _sitemapService.GetRoutes();
            var model = new DisplaySettingsPageModel {
                AutoLayout = false,
                Routes = routes.ToList()
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult DisplaySettings(DisplaySettingsPageModel model) {
            _sitemapService.SetRoutes(model.Routes);
            _notifier.Add(NotifyType.Information, T("Saved Sitemap display layout"));
            return RedirectToAction("DisplaySettings");
        }

        public ActionResult DeleteCustomRoute(string url, string returnUrl = null) {
            _sitemapService.DeleteCustomRoute(url);
            if (returnUrl == null) {
                return Content("");
            }
            return Redirect(returnUrl);
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

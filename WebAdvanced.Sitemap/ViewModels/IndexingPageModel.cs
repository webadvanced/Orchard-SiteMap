using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAdvanced.Sitemap.ViewModels {
    public class IndexingPageModel {
        public List<IndexSettingsModel> ContentTypeSettings { get; set; }
        public List<CustomRouteModel> CustomRoutes { get; set; }
    }
}

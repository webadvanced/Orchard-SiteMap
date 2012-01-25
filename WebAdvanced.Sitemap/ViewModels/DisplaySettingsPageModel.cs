using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAdvanced.Sitemap.ViewModels {
    public class DisplaySettingsPageModel {
        public List<RouteSettingsModel> Routes { get; set; }
        public bool AutoLayout { get; set; }
    }
}

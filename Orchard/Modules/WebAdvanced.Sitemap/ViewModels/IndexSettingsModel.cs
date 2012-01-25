using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace WebAdvanced.Sitemap.ViewModels {
    public class IndexSettingsModel {


        public string Name { get; set; }
        public string DisplayName { get; set; }
        public bool IndexForDisplay { get; set; }
        public bool IndexForXml { get; set; }
        public string UpdateFrequency { get; set; }
        public int Priority { get; set; }
    }
}

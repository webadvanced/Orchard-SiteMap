using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WebAdvanced.Sitemap.ViewModels {
    public class CustomRouteModel {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool IndexForDisplay { get; set; }
        public bool IndexForXml { get; set; }
        public string UpdateFrequency { get; set; }
        public int Priority { get; set; }
    }
}

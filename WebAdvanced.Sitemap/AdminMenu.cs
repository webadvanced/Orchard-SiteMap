using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Localization;
using Orchard.UI.Navigation;

namespace WebAdvanced.Sitemap {
    public class AdminMenu : INavigationProvider {

        public Localizer T { get; set; }
        public string MenuName { get { return "admin"; } }

        public void GetNavigation(NavigationBuilder builder) {
            builder.Add(T("Sitemap"), "9", menu => menu
                .Add(T("Indexing"), "1", item => item.Action("Indexing", "Admin", new {area = "WebAdvanced.Sitemap"}).LocalNav())
                .Add(T("Display"), "2", item => item.Action("DisplaySettings", "Admin", new {area = "WebAdvanced.Sitemap"}).LocalNav()));

        }
    }
}

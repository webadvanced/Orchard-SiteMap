
﻿using Orchard;

namespace WebAdvanced.Sitemap.Services {
    public interface ISitemapRouteFilter : IDependency {
        /// <summary>
        /// Filter function to disallow certain paths from appearing for display and xml.
        /// </summary>
        /// <param name="path">The relative path of the found content item.</param>
        /// <returns>True if this url should be allowed in the sitemap. False if not.</returns>
        bool AllowUrl(string path);
    }
}

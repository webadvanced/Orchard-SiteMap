using System.Collections.Generic;

namespace WebAdvanced.Sitemap.Services {
    public class DescribeSpecializedSitemapProviderContext {
        internal Dictionary<string, DescribeSpecializedSitemapFor> Describes { get; private set; }

        public DescribeSpecializedSitemapProviderContext() {
            Describes = new Dictionary<string, DescribeSpecializedSitemapFor>();
        }

        public DescribeSpecializedSitemapFor For(string namespacePrefix) {
            DescribeSpecializedSitemapFor describeFor;
            if (!Describes.TryGetValue(namespacePrefix, out describeFor)) {
                describeFor = new DescribeSpecializedSitemapFor(namespacePrefix);
                Describes[namespacePrefix] = describeFor;
            }
            return describeFor;
        }
    }
}
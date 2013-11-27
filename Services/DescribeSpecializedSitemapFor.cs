using System;
using System.Xml.Linq;
using Orchard.ContentManagement;

namespace WebAdvanced.Sitemap.Services {
    public class DescribeSpecializedSitemapFor {
        public string NamespacePrefix { get; private set; }
        public XNamespace XNamespace { get; private set; }
        public Func<ContentItem, string, XElement> Process { get; private set; }

        public DescribeSpecializedSitemapFor(string namespacePrefix) {
            NamespacePrefix = namespacePrefix;
        }

        public void Configure(XNamespace xNamespace, Func<ContentItem, string, XElement> process) {
            XNamespace = xNamespace;
            Process = process;
        }
    }
}
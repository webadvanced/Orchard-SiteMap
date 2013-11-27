using Orchard;

namespace WebAdvanced.Sitemap.Services {
    public interface ISpecializedSitemapProvider : IDependency {
        void Describe(DescribeSpecializedSitemapProviderContext providerContext);
    }
}
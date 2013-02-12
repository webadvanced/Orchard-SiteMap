using System.Collections.Generic;
using Orchard.Events;

namespace WebAdvanced.Sitemap.ImportExport
{
    public interface ICustomExportStep : IEventHandler
    {
        void Register(IList<string> steps);
    }

    public class SitemapCustomExportStep : ICustomExportStep
    {
        public const string ExportStep = "Sitemap";

        public void Register(IList<string> steps)
        {
            steps.Add(ExportStep);
        }
    }
}
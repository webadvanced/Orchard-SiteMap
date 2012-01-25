using System.Data;
using Orchard.Data.Migration;

namespace WebAdvanced.Sitemap {
    public class Migrations : DataMigrationImpl {
        public int Create() {
            SchemaBuilder.CreateTable("SitemapRouteRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Slug")
                .Column<int>("DisplayLevels", col => col.WithDefault(0))
                .Column<bool>("Active", col => col.WithDefault(true))
                .Column<int>("DisplayColumn", col => col.WithDefault(0))
                .Column<int>("Weight", col => col.WithDefault(0))
            );

            SchemaBuilder.CreateTable("SitemapSettingsRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("ContentType", col => col.Unique())
                .Column<bool>("IndexForDisplay", col => col.WithDefault(true))
                .Column<bool>("IndexForXml", col => col.WithDefault(true))
                .Column<string>("UpdateFrequency")
                .Column<int>("Priority")
            );

            SchemaBuilder.CreateTable("SitemapCustomRouteRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Url", col => col.Unique())
                .Column<bool>("IndexForDisplay", col => col.WithDefault(true))
                .Column<bool>("IndexForXml", col => col.WithDefault(true))
                .Column<string>("UpdateFrequency")
                .Column<int>("Priority")
            );

            return 2;
        }

        public int UpdateFrom1() {
            SchemaBuilder.CreateTable("SitemapCustomRouteRecord", table => table
                .Column<int>("Id", col => col.PrimaryKey().Identity())
                .Column<string>("Url", col => col.Unique())
                .Column<bool>("IndexForDisplay", col => col.WithDefault(true))
                .Column<bool>("IndexForXml", col => col.WithDefault(true))
                .Column<string>("UpdateFrequency")
                .Column<int>("Priority")
            );

            return 2;
        }
    }
}

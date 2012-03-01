using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Orchard.Environment.Extensions.Models;
using Orchard.Security.Permissions;

namespace WebAdvanced.Sitemap {
    public class Permissions : IPermissionProvider {
        public static readonly Permission ManageSitemap = new Permission { Description = "Manage sitemap", Name = "ManageSitemap" };

        public virtual Feature Feature { get; set; }

        public IEnumerable<Permission> GetPermissions() {
            return new[] {
                ManageSitemap,
            };
        }

        public IEnumerable<PermissionStereotype> GetDefaultStereotypes() {
            return new[] {
                new PermissionStereotype {
                    Name = "Administrator",
                    Permissions = new[] {ManageSitemap}
                }
            };
        }
    }
}

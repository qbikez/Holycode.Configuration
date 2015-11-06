using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;

namespace Microsoft.Framework.Configuration
{
    public static class ConnectionStringsExtenisons
    {
        /// <summary>
        /// extracts connections strings from config.json into ConnectionStringSettingsCollection (eg. ConfigurationManager.ConnectionStrings)
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="connectionStrings"> (eg. ConfigurationManager.ConnectionStrings)</param>
        public static void ExtractConnectionStrings(this IConfiguration cfg, ConnectionStringSettingsCollection connectionStrings)
        {
           var section = cfg.GetSection("connectionStrings");
            var subKeys = section.GetChildren();

            foreach (var subkey in subKeys)
            {
                var name = subkey.Key;
                var val = subkey.Value ?? subkey.Get("connectionString");
                var provider = subkey.Get("provider") ?? "System.Data.SqlClient";
                connectionStrings.AddConnectionString(name, val, provider);
            }
        }

        public static string GetConnectionStringValue(this IConfiguration cfg, string name)
        {
            var subkey = cfg.GetSection($"connectionStrings:{name}");
            if (subkey.Value == null && !subkey.GetChildren().Any())
            {
                subkey = cfg.GetSection($"{name}");
            }
            var val = subkey.Get("connectionString") ?? subkey.Value;
            return val;
            
        }
    }
}

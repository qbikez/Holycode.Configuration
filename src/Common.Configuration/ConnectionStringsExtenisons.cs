using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Framework.ConfigurationModel
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
            var subkeys = cfg.GetSubKeys("connectionStrings");
            foreach (var subkey in subkeys)
            {
                var name = subkey.Key;
                var val = subkey.Value.Get("connectionString") ?? subkey.Value.Get(null);
                var provider = subkey.Value.Get("provider") ?? "System.Data.SqlClient";
                connectionStrings.AddConnectionString(name, val, provider);
            }
        }

        public static string GetConnectionStringValue(this IConfiguration cfg, string name)
        {
            var subkey = cfg.GetSubKey($"connectionStrings:{name}");
            if (subkey.Get(null) == null && !subkey.GetSubKeys().Any())
            {
                subkey = cfg.GetSubKey($"{name}");
            }
            var val = subkey.Get("connectionString") ?? subkey.Get(null);
            return val;
        }
    }
}

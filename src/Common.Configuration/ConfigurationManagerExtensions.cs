#if !DNX451
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using System.Configuration;

namespace System.Configuration
{
    public static class ConfigurationManagerExtensions
    {
        public static void AddConnectionString(string name, string connectionString)
        {
            ConfigurationManager.ConnectionStrings.AddConnectionString(name, connectionString);
        }

        public static void AddConnectionString(ConnectionStringSettings connectionString)
        {
            ConfigurationManager.ConnectionStrings.AddConnectionString(connectionString);
        }

        public static void AddConnectionString(this ConnectionStringSettingsCollection cstrings,
            string name, string connectionstring, string providerName = "System.Data.SqlClient")
        {
            cstrings.AddConnectionString(new ConnectionStringSettings(name, connectionstring, providerName));
        }

        public static void AddConnectionString(this ConnectionStringSettingsCollection cstrings,
            ConnectionStringSettings connectionString)
        {
            typeof (ConfigurationElementCollection).GetField("bReadOnly", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(cstrings, false);
            if (cstrings[connectionString.Name] != null)
            {
                cstrings.Remove(connectionString.Name);
            }
            cstrings.Add(connectionString);
        }
    }

    public static partial class ConnectionStringsExtenisons
    {
        /// <summary>
        /// extracts connections strings from config.json into ConnectionStringSettingsCollection (eg. ConfigurationManager.ConnectionStrings)
        /// </summary>
        /// <param name="cfg"></param>
        /// <param name="connectionStrings"> (eg. ConfigurationManager.ConnectionStrings)</param>
        public static void ExtractConnectionStrings(this IConfiguration cfg,
            ConnectionStringSettingsCollection connectionStrings)
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
    }

}
#endif
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

}
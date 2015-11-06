#if !DNX451

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.ConfigurationModel;

namespace Common.Configuration
{
    public static class WebConfigExtensions
    {
        /// <summary>
        /// Adds the config values from web.config AppSettings and ConnectionStrings to IConfiguration, replacing '.' separator with ':'
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IConfiguration AddWebConfig(this IConfiguration configuration)
        {
            var keys = ConfigurationManager.AppSettings.AllKeys;

            foreach (var k in keys)
            {
                var val = ConfigurationManager.AppSettings[k];
                configuration.Set(k.Replace(".", ":"), val);
                var check = configuration.Get(k.Replace(".", ":"));
            }

            var connectionStrings = ConfigurationManager.ConnectionStrings;
            foreach (ConnectionStringSettings c in connectionStrings)
            {
                configuration.Set($"connectionStrings:{c.Name}:connectionString", c.ConnectionString);
                configuration.Set($"connectionStrings:{c.Name}:provider", c.ProviderName);
            }

            return configuration;
        }

     
        

        public static IConfiguration FillAppSettings(this IConfiguration configuration, string rootNs = "")
        {
            configuration.Traverse((key, val) => { ConfigurationManager.AppSettings[key.Replace(':','.')] = val; });
            return configuration;
        }
    }
}

#endif
#if !DNX451
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Configuration
{
    public static class ConfigurationExtensionsNet
    {
        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src)
        {
            var path = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            return src.AddEnvJson(path);
        }

        public static IConfigurationBuilder WithCallingAssemblyBasePath(
           this IConfigurationBuilder cfg)
        {
            var path = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            cfg.SetBasePath(path);
            return cfg;
        }
    }
}


#endif
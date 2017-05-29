#if !NETFX
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensionsVnext
    {        
        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src, bool optional = true, string environment = null)
        {
            var envPath = src.AppBasePath();
            return src.AddEnvJson(envPath, optional: optional, environment: environment);
        }

    }
}

#endif
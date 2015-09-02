#if DNX451
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.Runtime;

namespace Common.Configuration
{
    public static class ConfigurationExtensionsVnext
    {

        public static IHostingEnvironment UpdateEnvironment(this IHostingEnvironment env, IConfiguration configuration)
        {
            if (!string.IsNullOrEmpty(configuration.Get("ASPNET_ENV")))
                env.EnvironmentName = configuration.Get("ASPNET_ENV");
            return env;
        }

        public static IConfigurationSourceRoot AddEnvJson(this IConfigurationSourceRoot src, IApplicationEnvironment env)
        {
            var envPath = env.ApplicationBasePath;
            return src.AddEnvJson(envPath);
        }

    }
}

#endif
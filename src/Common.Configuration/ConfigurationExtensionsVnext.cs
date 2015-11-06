#if DNX451
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Dnx.Runtime;
using Microsoft.Framework.Configuration;

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

        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src, IApplicationEnvironment env)
        {
            var envPath = env.ApplicationBasePath;
            return src.AddEnvJson(envPath);
        }

        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src)
        {
            var envPath = src.GetBasePath();
            return src.AddEnvJson(envPath);
        }

        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src, bool optional = true)
        {
            var envPath = src.GetBasePath();
            return src.AddEnvJson(envPath, optional: optional);
        }

    }
}

#endif
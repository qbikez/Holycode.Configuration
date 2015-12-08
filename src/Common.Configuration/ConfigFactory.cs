using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Configuration
{
    public class ConfigFactory
    {
#if !DNX451

        public static IConfigurationBuilder CreateConfigSource()
        {
            return CreateConfigSource(Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length));
        }
#endif
        public static IConfigurationBuilder CreateConfigSource(string applicationBasePath, bool addEnvVariables = true)
        {
            if (applicationBasePath == null)
            {
                throw new Exception("could not resolve applicationBasePath from calling assembly codebase");
            }
            if (File.Exists(applicationBasePath))
                applicationBasePath = Path.GetDirectoryName(applicationBasePath);

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            if (addEnvVariables) builder.AddEnvironmentVariables();
            builder.AddInMemoryCollection();
            builder.SetBasePath(applicationBasePath);


            if (builder.Get(ConfigurationExtensions.ApplicationBasePathKey) == null)
                builder.Set(ConfigurationExtensions.ApplicationBasePathKey, applicationBasePath);

            return builder;
        }

        public static IConfiguration Create(string applicationBasePath = null, bool addEnvVariables = true)
        {
            return CreateConfigSource(applicationBasePath, addEnvVariables).Build();
        }

        public static IConfiguration FromEnvJson(string applicationBasePath = null, bool addEnvVariables = true)
        {

            var config = CreateConfigSource(applicationBasePath, addEnvVariables);

            applicationBasePath = config.GetBasePath();
            if (applicationBasePath == null)
            {
                applicationBasePath = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
                if (File.Exists(applicationBasePath))
                    applicationBasePath = Path.GetDirectoryName(applicationBasePath);
                config.SetBasePath(applicationBasePath);
            }

            config = config.AddEnvJson(config.GetBasePath());

            return config.Build();
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.ConfigurationModel;

namespace Common.Configuration
{
    public class ConfigFactory
    {

        public static IConfigurationSourceRoot CreateConfigSource(string applicationBasePath = null)
        {
            IConfigurationSourceRoot config = null;
            if (applicationBasePath == null)
                applicationBasePath = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            if (applicationBasePath == null)
            {
                throw new Exception("could not resolve applicationBasePath from calling assembly codebase");
            }
            if (File.Exists(applicationBasePath))
                applicationBasePath = Path.GetDirectoryName(applicationBasePath);

#if DNX451
                config = new Microsoft.Framework.ConfigurationModel.Configuration().AddEnvironmentVariables();
#else 
                /// this ctor is added in a newer version of ConfigurationModel than the one that is referenced by DNX451
                config = new Microsoft.Framework.ConfigurationModel.Configuration(applicationBasePath).AddEnvironmentVariables();
#endif
            if (config.Get(ConfigurationExtensions.ApplicationBasePathKey) == null)
                config.Set(ConfigurationExtensions.ApplicationBasePathKey, applicationBasePath);

            return config;
        }

        public static IConfiguration Create(string applicationBasePath = null)
        {
            return CreateConfigSource(applicationBasePath);
        }

        public static IConfiguration FromEnvJson(string applicationBasePath = null)
        {

            var config = CreateConfigSource(applicationBasePath);
#if DNX451
            if (applicationBasePath == null)
                applicationBasePath = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            if (File.Exists(applicationBasePath))
                applicationBasePath = Path.GetDirectoryName(applicationBasePath);

            config = config.AddEnvJson(applicationBasePath);
#else
            config = config.AddEnvJson(config.BasePath);            
#endif

            return config;
        }
    }
}

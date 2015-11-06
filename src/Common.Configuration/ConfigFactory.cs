using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.Configuration;

namespace Common.Configuration
{
    public class ConfigFactory
    {
#if !DNX451
        public static IConfigurationBuilder CreateConfigSource() : this(Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length))
        {            
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

#if DNX451
            var builder = new Microsoft.Framework.Configuration.ConfigurationBuilder();
            if (addEnvVariables) builder.AddEnvironmentVariables();
            builder.AddInMemoryCollection();
            builder.SetBasePath(applicationBasePath);
            //config = builder.Build();
#else
                /// this ctor is added in a newer version of ConfigurationModel than the one that is referenced by DNX451
                //config = new Microsoft.Framework.ConfigurationModel.Configuration(applicationBasePath).AddEnvironmentVariables();
#endif
            
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
#if DNX451
            if (applicationBasePath == null)
                applicationBasePath = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            if (File.Exists(applicationBasePath))
                applicationBasePath = Path.GetDirectoryName(applicationBasePath);

            config = config.AddEnvJson(applicationBasePath);
#else
            config = config.AddEnvJson(config.BasePath);            
#endif

            return config.Build();
        }
    }
}

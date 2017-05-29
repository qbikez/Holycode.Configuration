using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.Configuration
{
    public class ConfigFactory
    {
#if NETFX

        public static IConfigurationBuilder CreateConfigSource()
        {
            return CreateConfigSource(Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length));
        }
#endif
        public static IConfigurationBuilder CreateConfigSource(string applicationBasePath, bool addEnvVariables = true)
        {
            if (applicationBasePath == null)
            {
                // this may return something like c:\windows\.. when running in ASP
#if !CORECLR
                // var asm = Assembly.GetCallingAssembly();
                // if (asm.GetName().Name != typeof (ConfigFactory).Assembly.GetName().Name)
                // {
                //     applicationBasePath = asm.CodeBase.Substring("file:///".Length);
                // }
                // else
                // {
                //     applicationBasePath = Path.GetFullPath(".");
                // }
                applicationBasePath = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath;

#else
                applicationBasePath = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath;
#endif
            }

            if (applicationBasePath != null && File.Exists(applicationBasePath))
                applicationBasePath = Path.GetDirectoryName(applicationBasePath);

            var builder = new Microsoft.Extensions.Configuration.ConfigurationBuilder();
            if (addEnvVariables) builder.AddEnvironmentVariables();
            builder.AddInMemoryCollection();
            
            builder.SetAppBasePath(applicationBasePath);

            return builder;
        }

        public static IConfiguration Create(string applicationBasePath = null, bool addEnvVariables = true)
        {
            return CreateConfigSource(applicationBasePath, addEnvVariables).Build();
        }

        public static IConfiguration FromEnvJson(string applicationBasePath = null, bool addEnvVariables = true, string environment = null)
        {

            var config = CreateConfigSource(applicationBasePath, addEnvVariables);

            applicationBasePath = config.AppBasePath();
            if (applicationBasePath == null)
            {
#if !CORECLR
                applicationBasePath = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
#else
                throw new Exception("could not resolve applicationBasePath from calling assembly codebase");
#endif

                if (File.Exists(applicationBasePath))
                    applicationBasePath = Path.GetDirectoryName(applicationBasePath);
                config.SetAppBasePath(applicationBasePath);
            }

            config = config.AddEnvJson(config.AppBasePath(), optional: false, environment: environment);

            return config.Build();
        }
    }
}

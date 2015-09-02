#if !DNX451
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.ConfigurationModel.Internal;

namespace Common.Configuration
{
    public static class ConfigurationExtensionsNet
    {
        public static IConfigurationSourceRoot AddEnvJson(this IConfigurationSourceRoot src)
        {
            var path = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
            return src.AddEnvJson(path);
        }

        public static Microsoft.Framework.ConfigurationModel.Configuration WithCallingAssemblyBasePath(
           this Microsoft.Framework.ConfigurationModel.Configuration cfg)
        {
            var path = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);

            return new Microsoft.Framework.ConfigurationModel.Configuration(basePath: path);
        }
    }
}


#endif
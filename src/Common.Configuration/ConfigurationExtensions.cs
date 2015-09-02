using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Configuration;

namespace Microsoft.Framework.ConfigurationModel
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationSourceRoot AddEnvJson(this IConfigurationSourceRoot src, string applicationBasePath)
        {
            return AddEnvJson(src, applicationBasePath, optional: true);
        }
        public static IConfigurationSourceRoot AddEnvJson(this IConfigurationSourceRoot src, string applicationBasePath, bool optional)
        {
            var envPath = ServicePathResolver.ExtractNames(applicationBasePath).FirstOrDefault();
            if (envPath != null)
            {
                var path = Path.Combine(envPath.Source, "env.json");
                src = src.AddJsonFile(path, optional: optional);
                src.Set("env:config:path", path);
                src.Set("env:config:found", File.Exists(path).ToString());
                if (src.Get("application:basePath") == null)
                    src.Set("application:basePath", applicationBasePath);

                var environment = src.Get("ASPNET_ENV");

                /// add env.qa.json, etc
                if (!string.IsNullOrEmpty(environment))
                {
                    path = Path.Combine(envPath.Source, $"env.{environment}.json");
                    if (File.Exists(path))
                    {
                        src = src.AddJsonFile(path, optional: optional);
                        src.Set("env:config:path", path);
                        src.Set("env:config:found", "true");
                    }
                }

            }
            else
            {

            }

            return src;
        }


        public static Nullable<T> GetNullable<T>(this IConfiguration cfg, string key)
          where T : struct
        {
            var v = (cfg.Get(key));
            if (v == null) return null;
            else return (T)Convert.ChangeType(v, typeof(T));
        }

        public static Dictionary<string, string> GetDictionary(this IConfiguration cfg, string key)
        {
            var subkeys = cfg.GetSubKeys(key);
            if (subkeys == null) return null;
            var d = new Dictionary<string, string>();
            foreach (var pair in subkeys)
            {
                d.Add(pair.Key, pair.Value.Get(null));
            }
            return d;
        }

        public static T TryGet<T>(this IConfiguration cfg, string key)
           where T : struct
        {
            var v = (cfg.Get(key));
            if (v == null) return default(T);
            else return cfg.Get<T>(key);
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Common.Configuration;
using Microsoft.Framework.ConfigurationModel.Internal;

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

                src = src.AddJsonFile($"env.local.json", optional: true);
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
            if (string.IsNullOrEmpty(v)) return null;
            if (v.Equals("false", StringComparison.InvariantCultureIgnoreCase) && typeof(T) != typeof(bool)) return null;
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

        public static void Traverse(this IConfiguration configuration, Action<string, string> action, string rootNs = "")
        {
            var keys = configuration.GetSubKeys();

            string val = null;
            if (configuration is ConfigurationFocus)
            {
                val = ((ConfigurationFocus)configuration).Get(null);
            }
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    Traverse(key.Value, action, rootNs + ":" + key.Key);
                }
            }
            if (val != null)
            {
                var key = rootNs.TrimStart(':');
                action(key, val);
            }
        }

        public static TResult Aggregate<TResult>(this IConfiguration configuration,
            TResult seed,
            Func<TResult, string, object, TResult> action, string rootNs = "")
        {
            return Aggregate(configuration, action, rootNs);
        }
        private static TResult Aggregate<TResult>(this IConfiguration configuration, Func<TResult, string, object, TResult> action, string rootNs = "")
        {
            var keys = configuration.GetSubKeys();

            string val = null;
           
            TResult ag = default(TResult);
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    var sub = key.Value;
                    if (sub is ConfigurationFocus)
                    {
                        val = ((ConfigurationFocus)sub).Get(null);
                    }
                    if (val != null)
                    {
                        ag = action(ag, rootNs + ":" + key.Key, val);
                    }
                    else
                    {
                        var subag = Aggregate(key.Value, action, rootNs + ":" + key.Key);
                        ag = action(ag, rootNs + ":" + key.Key, subag);
                    }
                }
            }
            

            return ag;
        }
        public static Dictionary<string, string> AsDictionaryPlain(this IConfiguration config)
        {
            var dict = new Dictionary<string, string>();
            config.Traverse((s, s1) =>
            {
                dict.Add(s,s1);
            });

            return dict;
        }

        public static Dictionary<string, object> AsDictionaryNested(this IConfiguration config)
        {
            return config.Aggregate(new Dictionary<string, object>(),
                (dict, key, val) =>
                {
                    if (dict == null) dict = new Dictionary<string, object>();
                    key = key.Split(':').Last();
                    dict.Add(key, val);
                    return dict;
                });
        }
    }
}

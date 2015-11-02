using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
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

                src = src.AddJsonFile(Path.Combine(envPath.Source, $"env.local.json"), optional: true);
            }
            else
            {

            }

            return src;
        }


        public static Nullable<T> GetNullable<T>(this IConfiguration cfg, string key, Func<string, T> convert = null)
          where T : struct
        {
            var v = (cfg.Get(key));
            if (string.IsNullOrEmpty(v)) return null;
            if (v.Equals("false", StringComparison.InvariantCultureIgnoreCase) && typeof(T) != typeof(bool))
            {
                return null;
            }
            else
            {
                if (convert != null) return convert(v);
                return (T)Convert.ChangeType(v, typeof(T));
            }
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

        /// <summary>
        /// Gets the secure service URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="baseUrl">The base URL.</param>
        /// <returns></returns>
        public static string GetSecureServiceUrl(this IConfiguration config, string url, string baseUrl = null, int? sslPort = null)
        {
            if (url == null) return null;
            url = config.GetServiceUrl(url, baseUrl);
            if (sslPort == null)
            {
                var uri = new Uri(url);
                if (uri.Port == 80) sslPort = 443;
                else throw new ArgumentException($"base url uses non-standard port {uri.Port}. Cannot determine ssl port - no sslPort given and 'sslPort' key not found in config");
            }
            
            if (!url.StartsWith("https://"))
            {
                // do not use https for localhost development
                if (url.StartsWith("http://localhost"))
                {
                    return url;
                }
                url = url.Replace("http://", "https://");
                url = Regex.Replace(url, @"(\:[0-9]+)/", $":{sslPort.Value}/");
            }

            return url;
        }

        public static string GetServiceUrl(this IConfiguration config, string url, string baseUrl = null)
        {
            if (baseUrl == null) baseUrl = config.Get("baseUrl");
            if (baseUrl == null) throw new ArgumentException("no baseUrl given and 'baseUrl' key not found in config");
            if (url == null) return null;
            /// is url relative?
            if (url.StartsWith("/"))
            {
                return (baseUrl.TrimEnd('/') ?? "") + url;
            }
            else
            {
                return url;
            }
        }

        public static IEnumerable<IConfigurationSource> GetSources(this IConfigurationSourceRoot root, string key)
        {
            var src = root.Sources.Where(s =>
            {
                string tmp;
                return s.TryGet(key, out tmp);
            });

            return src;
        }
    }
}

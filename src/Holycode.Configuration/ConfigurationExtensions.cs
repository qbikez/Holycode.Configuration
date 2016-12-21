using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Holycode.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        internal const string EnvConfigFoundKey = "env:config:found";
        internal const string EnvConfigPathKey = "env:config:path";
        internal const string ApplicationBasePathKey = "application:basePath";
        internal const string EnvironmentNameKey = "ASPNET_ENV";
        private static readonly string DefaultEnvironment = "development";

        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src, string applicationBasePath)
        {
            return AddEnvJson(src, applicationBasePath, optional: true);
        }

        /// <summary>
        /// Looks for env.json and env.{environment}.json and adds them to config
        /// </summary>
        /// <param name="src">configuration builder</param>
        /// <param name="applicationBasePath">application base path</param>
        /// <param name="optional">is env.json optional</param>
        /// <param name="environment">force specific environment (overwrites  value provided by env.json)[optional]</param>
        /// <returns></returns>
        public static IConfigurationBuilder AddEnvJson(this IConfigurationBuilder src, string applicationBasePath, bool optional, string environment = null)
        {

            if (src.Get(ApplicationBasePathKey) == null)
                src.Set(ApplicationBasePathKey, applicationBasePath);

            var envPath = ServicePathResolver.ExtractNames(applicationBasePath).FirstOrDefault();
            if (envPath != null)
            {
                var path = Path.Combine(envPath.Source, "env.json");
                try
                {
                    src = src.AddJsonFile(path, optional: optional);
                }
                catch (Exception ex)
                {
                    throw new FileLoadException($"Failed to load config file {path}", ex);
                }
                src.Set(EnvConfigPathKey, path);
                src.Set(EnvConfigFoundKey, File.Exists(path).ToString());

                try
                {
                    path = Path.Combine(envPath.Source, $"env.override.json");
                    src = src.AddJsonFile(path, optional: true);
                }
                catch (Exception ex)
                {
                    throw new FileLoadException($"Failed to load config file {path}", ex);
                }


                environment = environment ?? src.Get(EnvironmentNameKey) ?? DefaultEnvironment;

                // force env
                src.Set(EnvironmentNameKey, environment);

                /// add env.qa.json, etc
                if (!string.IsNullOrEmpty(environment))
                {
                    path = Path.Combine(envPath.Source, $"env.{environment}.json");
                    if (File.Exists(path) || !optional)
                    {
                        try
                        {
                            src = src.AddJsonFile(path, optional: optional);
                        }
                        catch (Exception ex)
                        {
                            throw new FileLoadException($"Failed to load config file {path}", ex);
                        }

                        src.Set(EnvConfigPathKey, path);
                        src.Set(EnvConfigFoundKey, "true");
                    }
                }

              
                if (!string.IsNullOrEmpty(environment))
                {
                    try
                    {
                        path = Path.Combine(envPath.Source, $"env.{environment}.override.json");
                        src = src.AddJsonFile(path, optional: true);
                        path = Path.Combine(envPath.Source, $"env.{environment}.local.json");
                        if (File.Exists(path)) src = src.AddJsonFile(path, optional: true);
                    }
                    catch (Exception ex)
                    {
                        throw new FileLoadException($"Failed to load config file {path}", ex);
                    }
                }

            }
            else
            {

            }

            return src;
        }


        public static IConfigurationBuilder Set(this IConfigurationBuilder builder, string key, string value)
        {
            builder.Properties[key] = value;
            var mem = builder.Sources.Where(p => p is MemoryConfigurationSource).FirstOrDefault() as MemoryConfigurationSource;
            if (mem != null)
            {
                var data = mem.InitialData?.ToList() ?? new List<KeyValuePair<string, string>>();
                data.RemoveAll(d => d.Key == key);
                data.Add(new KeyValuePair<string, string>(key, value));
                mem.InitialData = data;
            }
            return builder;
        }

        public static string Get(this IConfigurationBuilder builder, string key)
        {
            object val;
            if (builder.Properties.TryGetValue(key, out val))
            {
                return val.ToString();
            }
            //foreach (var src in builder.Sources)
            //{
            //    string strVal;

            //    if (src.Build(builder).TryGet(key, out strVal))
            //    {
            //        return strVal;
            //    }
            //}
            return builder.Build().Get(key);
        }

        public static T? GetNullable<T>(this IConfiguration cfg, string key, Func<string, T> convert = null)
          where T : struct
        {
            var v = (cfg.Get(key));
            if (string.IsNullOrEmpty(v)) return null;
            if (v.Equals("false", StringComparison.CurrentCultureIgnoreCase) && typeof(T) != typeof(bool))
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
            var subkeys = cfg.GetSection(key).GetChildren();
            if (subkeys == null) return null;
            var d = new Dictionary<string, string>();
            foreach (var pair in subkeys)
            {
                d.Add(pair.Key, pair.Value);
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

        public static string Get(this IConfiguration configuration, string key)
        {
            return configuration[key];
        }

        public static void Set(this IConfiguration configuration, string key, string value)
        {
            configuration[key] = value;
        }

        public static T Get<T>(this IConfiguration configuration, string key)
        {
            return (T)Convert.ChangeType(configuration[key], typeof(T));
        }


        public static void Traverse(this IConfiguration configuration, Action<string, string> action, string rootNs = "")
        {
            var keys = configuration.GetChildren();

            string val = null;
            if (configuration is IConfigurationSection)
            {
                val = ((IConfigurationSection)configuration).Value;
            }
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    Traverse(key, action, rootNs + ":" + key.Key);
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
            var keys = configuration.GetChildren();


            string val = null;

            TResult ag = default(TResult);
            if (keys != null)
            {
                foreach (var key in keys)
                {
                    val = key.Value;
                    if (val != null)
                    {
                        ag = action(ag, rootNs + ":" + key.Key, val);
                    }
                    else
                    {
                        var subag = Aggregate(key, action, rootNs + ":" + key.Key);
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
                dict.Add(s, s1);
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

        public static string EnvironmentName(this IConfiguration cfg)
        {
            return cfg.Get(EnvironmentNameKey);
        }

        public static string EnvironmentName(this IConfigurationBuilder cfg)
        {
            return cfg.Get(EnvironmentNameKey);
        }

        public static string BasePath(this IConfiguration cfg)
        {
            return cfg.Get(ApplicationBasePathKey);
        }

        public static string BasePath(this IConfigurationBuilder cfg)
        {
            return cfg.Get(ApplicationBasePathKey);
        }


        /*

        public static IEnumerable<IConfigurationRoot> GetSources(this IConfigurationRoot root, string key)
        {
            var src = root.Sources.Where(s =>
            {
                string tmp;
                return s.TryGet(key, out tmp);
            });

            return src;
        }
        */
    }
}

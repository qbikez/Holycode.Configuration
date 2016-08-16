using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Holycode.Configuration;
using System;
using System.Collections.Generic;

namespace Holycode.Configuration.Commands
{
    public class Program
    {
        private static bool verbose = false;
        private static bool noEnvVar = false;

        public static int Main(string[] args)
        {
            //Console.WriteLine($"args={string.Join(" ", args)}");

            try
            {
                string env = null;
                string cmd = null;
                string path = null;
                int noDashArgIdx = 0;
                for (int i = 0; i < args.Length; i++)
                {
                    if (!args[i].StartsWith("-"))
                    {
                        switch (noDashArgIdx)
                        {
                            case 0:
                                cmd = args[i]; break;
                            case 1:
                                path = args[i]; break;
                        }
                        noDashArgIdx++;

                        continue;
                    }
                    if (args[i].Equals("--env", StringComparison.OrdinalIgnoreCase) ||
                        args[i].Equals("-env", StringComparison.OrdinalIgnoreCase))
                    {
                        if (i == args.Length - 1 || args[i + 1].StartsWith("-"))
                        {
                            throw new ArgumentException("arg --env requires a value!");
                        }
                        env = args[i + 1];
                        i++;
                        continue;
                    }
                    if (args[i].Equals("--verbose", StringComparison.OrdinalIgnoreCase))
                    {
                        verbose = true;
                        continue;
                    }
                    if (args[i].Equals("--no-env-var", StringComparison.OrdinalIgnoreCase))
                    {
                        noEnvVar = true;
                        continue;
                    }
                }

                //Console.WriteLine($"looking for env.config... environment={env}");
                if (cmd == null)
                {
                    Console.WriteLine("Available commands:");
                    Console.WriteLine(" get              returns whole configuration, serialized as JSON");
                    Console.WriteLine(" list             list configuration keys");
                    Console.WriteLine(" get {key}        returns config value for key {key}");
                    Console.WriteLine(" connstr {name}   returns connection string with name {name}");
                    return 0;
                }

                var builder = ConfigFactory.CreateConfigSource(Directory.GetCurrentDirectory());
                builder.AddEnvJson(environment: env, optional: false);
                builder.AddJsonFile("config.json", optional: true);
                builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: true);
                 
                var conf = builder.Build();

                if (cmd == "get")
                {
                    if (path == null)
                    {
                        ListConfigValues(conf, builder);
                    }
                    else
                    {
                        var val = conf.Get(path);
                        if (val == null)
                        {
                            var sub = conf.GetSection(path);
                            if (sub != null)
                            {
                                ListConfigValues(sub, builder);
                            }
                        }
                        Console.WriteLine(val);
                    }

                    return 0;
                }
                else if (cmd == "list")
                {
                    ListConfigKeys(conf, builder);
                }

                if (cmd.Equals("connstr", StringComparison.OrdinalIgnoreCase)
                || cmd.Equals("getconnstr", StringComparison.OrdinalIgnoreCase)
                || cmd.Equals("conn", StringComparison.OrdinalIgnoreCase))
                {
                    if (path == null)
                    {
                        Console.WriteLine("Error: expected connstr name!");
                        return -1;
                    }
                    else
                    {
                        var val = conf.GetConnectionStringValue(path);
                        Console.WriteLine(val);
                    }

                    return 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message} {ex.StackTrace}");
                return -1;
            }

            return 0;
        }


        private static void ListConfigKeys(IConfiguration conf, IConfigurationBuilder builder)
        {
            var dict = conf.AsDictionaryPlain();
            foreach (var k in dict.Keys)
            {
                Console.WriteLine(k);
            }
        }

        private static Dictionary<TKey, TVal> BuildAnonymousDict<TKey, TVal>(Func<TVal> factory) {
            return new Dictionary<TKey, TVal>();
        }
        private static Dictionary<string, TVal> BuildAnonymousDict<TVal>(Func<TVal> factory) {
            return new Dictionary<string, TVal>();
        }
        private static void ListConfigValues(IConfiguration conf, IConfigurationBuilder builder)
        {
            var result = BuildAnonymousDict(() => new {Value = "", Source = "" });
            var dict = conf.AsDictionaryPlain();
            if (verbose) {
                foreach(var src in builder.Sources.Reverse())
                {
                    var srcName = src.ToString();
                    if (src is Microsoft.Extensions.Configuration.Json.JsonConfigurationSource)
                    {
                        var json = (src as Microsoft.Extensions.Configuration.Json.JsonConfigurationSource);
                        srcName = $"{json.Path} ({json.FileProvider.GetFileInfo(json.Path).PhysicalPath})";
                    }
                    if (
                        src is
                            Microsoft.Extensions.Configuration.EnvironmentVariables.
                                EnvironmentVariablesConfigurationSource)
                    {
                        srcName = "ENV";
                        if (noEnvVar)
                        {
                            continue;
                        }
                    }
                    if (src is Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource)
                    {
                        srcName = "in-mem";
                    }

                    var count = 0;
                    //Console.WriteLine($"examining source '{srcName}'");
                    var srcCfg = src.Build(builder);
                    srcCfg.Load();
                    foreach(var key in dict.Keys) {
                        string v = null;
                        if (srcCfg.TryGet(key, out v))
                        {
                            count++;
                            if (!result.ContainsKey(key)) {
                                result.Add(key, new { Value = v, Source = srcName });
                            }                            
                        }
                    }
                    //Console.WriteLine($"source '{srcName}' has {count} entries");
                }
                var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(result,
                    Newtonsoft.Json.Formatting.Indented);
                Console.WriteLine(serialized);
            } else {
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(dict,
                Newtonsoft.Json.Formatting.Indented);
            Console.WriteLine(serialized);
            }
        }
    }
}

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
                string cfgPath = null;
                string diskPath = null;
                List<string> nodashArgs = new List<string>();
                for (int i = 0; i < args.Length; i++)
                {
                    if (!args[i].StartsWith("-"))
                    {
                        nodashArgs.Add(args[i]);                        
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

                if (nodashArgs.Count > 0) {
                    var diskPathIdx = 0;
                    var cmdIdx = 1;
                    var cfgPathIdx = 2;
                    if (nodashArgs.Count < 3)
                    {                        
                        if (System.IO.File.Exists(nodashArgs[0]) || System.IO.Directory.Exists(nodashArgs[0]) || nodashArgs[0].Contains("/") || nodashArgs.Contains("\\"))
                        {
                            // first argument seems like a disk path                                                       
                        } else {
                            // first argument seems like a cmd
                            diskPathIdx = -1;
                            cmdIdx = 0;
                            cfgPathIdx = 1;
                        }
                    } 

                    diskPath = diskPathIdx >= 0 && diskPathIdx < nodashArgs.Count ? nodashArgs[diskPathIdx] : ".";
                    cmd =  cmdIdx >= 0 && cmdIdx < nodashArgs.Count ? nodashArgs[cmdIdx] : null;
                    cfgPath = cfgPathIdx >= 0 && cfgPathIdx < nodashArgs.Count ? nodashArgs[cfgPathIdx] : null;
                }

                //Console.WriteLine($"looking for env.config... environment={env}");
                if (cmd == null)
                {
                    System.Console.WriteLine("Usage:");
                    System.Console.WriteLine("hfcg [path] <command>");
                    System.Console.WriteLine();
                    Console.WriteLine("Available commands:");
                    Console.WriteLine(" get              returns whole configuration, serialized as JSON");
                    Console.WriteLine(" list             list configuration keys");
                    Console.WriteLine(" get {key}        returns config value for key {key}");
                    Console.WriteLine(" connstr {name}   returns connection string with name {name}");
                    Console.WriteLine(" tree             shows config tree");
                    return 0;
                }

                var builder = ConfigFactory.CreateConfigSource(Path.GetFullPath(diskPath));
                builder.AddEnvJson(environment: env, optional: false);
                builder.AddJsonFile("config.json", optional: true);
                builder.AddJsonFile($"config.{builder.EnvironmentName()}.json", optional: true);
                
                var conf = builder.Build();

                if (cmd == "get")
                {
                    if (cfgPath == null)
                    {
                        ListConfigValues(conf, builder);
                    }
                    else
                    {
                        var val = conf.Get(cfgPath);
                        if (val == null)
                        {
                            var sub = conf.GetSection(cfgPath);
                            
                            if (sub != null)
                            {
                                ListConfigValues(sub, builder);
                            } else {
                                return 1;
                            }
                        } else {
                            Console.WriteLine(val);
                        }
                    }

                    return 0;
                }
                else if (cmd == "list")
                {
                    System.Console.WriteLine($"base path: {conf.AppBasePath()}"); 
                    System.Console.WriteLine();
                    ListConfigKeys(conf, builder);
                    return 0;
                }
                else if (cmd.Equals("connstr", StringComparison.OrdinalIgnoreCase)
                || cmd.Equals("getconnstr", StringComparison.OrdinalIgnoreCase)
                || cmd.Equals("conn", StringComparison.OrdinalIgnoreCase))
                {
                    if (cfgPath == null)
                    {
                        Console.WriteLine("Error: expected connstr name!");
                        return -1;
                    }
                    else
                    {
                        var val = conf.GetConnectionStringValue(cfgPath);
                        Console.WriteLine(val);
                    }

                    return 0;
                }
                else if (cmd.Equals("tree", StringComparison.OrdinalIgnoreCase)){
                    System.Console.WriteLine($"base path: {conf.AppBasePath()}"); 
                    System.Console.WriteLine();
                    ShowConfigTree(builder);
                } 
                else {
                    System.Console.WriteLine($"Unknown command: '{cmd}'"); 
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

        private static Dictionary<TKey, TVal> BuildAnonymousDict<TKey, TVal>(Func<TVal> factory)
        {
            return new Dictionary<TKey, TVal>();
        }
        private static Dictionary<string, TVal> BuildAnonymousDict<TVal>(Func<TVal> factory)
        {
            return new Dictionary<string, TVal>();
        }
        private static Dictionary<TKey, TVal> BuildDict<TKey, TVal>(IEnumerable<KeyValuePair<TKey, TVal>> list)
        {
            return list.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }


        class ConfigSourceInfo {
            public string Path;
            public string Name;
            public string FullPath;
        }
        private static ConfigSourceInfo GetConfigSourceInfo(IConfigurationSource src) {
            var srcName = src.ToString();
            if (src is Microsoft.Extensions.Configuration.Json.JsonConfigurationSource)
            {
                var json = (src as Microsoft.Extensions.Configuration.Json.JsonConfigurationSource);
                 return new ConfigSourceInfo() {
                    Name = $"{json.Path} ({json.FileProvider.GetFileInfo(json.Path).PhysicalPath})",
                    Path = json.Path,
                    FullPath = json.FileProvider.GetFileInfo(json.Path).PhysicalPath
                };
            }
            else if (
                src is
                    Microsoft.Extensions.Configuration.EnvironmentVariables.
                        EnvironmentVariablesConfigurationSource)
            {
                  return new ConfigSourceInfo() {
                    Name = "ENV" 
                };
            }
            else if (src is Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource)
            {
                 return new ConfigSourceInfo() {
                    Name = "in-mem" 
                };
            }
            else {
                 return new ConfigSourceInfo() {
                    Name = src.ToString()
                };
            }
            
        }

        private static void ShowConfigTree(IConfigurationBuilder builder) {
            
            Stack<ConfigSourceInfo> parents = new Stack<ConfigSourceInfo>();
            foreach (var src in builder.Sources) {
                var srcInfo = GetConfigSourceInfo(src); 
                if (srcInfo.Path != null) {
                    var fn = Path.GetFileNameWithoutExtension(srcInfo.Path);
                    while (parents.Count > 0) {
                        var p = parents.Peek();
                        var pfn = Path.GetFileNameWithoutExtension(p.Path);
                        if (fn.StartsWith(pfn)) {
                            // fn is a child of pfn
                            break;
                        }
                        else {
                            parents.Pop();
                        }
                    }
                    
                    System.Console.Write("  ");
                    if (File.Exists(srcInfo.FullPath)) {
                        System.Console.Write("[√]");
                    } else {
                        System.Console.Write("[×]");
                    }
                    System.Console.Write("  ");

                    if (parents.Count > 0) {
                        System.Console.Write("└╴".PadLeft(parents.Count * 2));
                    } 
                    
                    System.Console.Write(srcInfo.Path);                    
                    System.Console.Write("\t\t");
                    System.Console.Write($"({srcInfo.FullPath})");  
                    
                    System.Console.WriteLine();     
                    parents.Push(srcInfo);
                }
            }
            System.Console.WriteLine();
        }

        private static void ListConfigValues(IConfiguration conf, IConfigurationBuilder builder)
        {
            var result = BuildAnonymousDict(() => new { Value = "", Source = "" });
            var dict = conf.AsDictionaryPlain();
            
            foreach (var src in builder.Sources.Reverse())
            {
                var srcName = GetConfigSourceInfo(src).Name;

                var count = 0;
                //Console.WriteLine($"examining source '{srcName}'");
                var srcCfg = src.Build(builder);
                srcCfg.Load();
                foreach (var k in dict.Keys)
                {
                    var key = k;
                    string v = null;
                    if (conf is IConfigurationSection) {
                        key = (conf as IConfigurationSection).Path + ":" + key;
                    }
                    if (srcCfg.TryGet(key, out v))
                    {
                        count++;
                        if (!result.ContainsKey(key))
                        {
                            result.Add(key, new { Value = v, Source = srcName });
                        }
                    }
                }
                //Console.WriteLine($"source '{srcName}' has {count} entries");
            }

            if (noEnvVar)
            {
                result = BuildDict(result.Where(kvp => kvp.Value.Source != "ENV"));
            }

            string serialized = null;
            if (verbose)
            {
                serialized = Newtonsoft.Json.JsonConvert.SerializeObject(result,
                    Newtonsoft.Json.Formatting.Indented);
            }
            else
            {
                var r2 = BuildDict(result.Select(kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value.Value)));
                serialized = Newtonsoft.Json.JsonConvert.SerializeObject(r2,
                    Newtonsoft.Json.Formatting.Indented);
            }
            Console.WriteLine(serialized);
        }

    }
}

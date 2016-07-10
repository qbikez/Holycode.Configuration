using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Holycode.Configuration;

namespace Holycode.Configuration.Commands
{
    public class Program
    {


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
                }

                //Console.WriteLine($"looking for env.config... environment={env}");
                if (cmd == null)
                {
                    Console.WriteLine("Available commands:");
                    Console.WriteLine(" get              returns whole configuration, serialized as JSON");
                    Console.WriteLine(" get {key}        returns config value for key {key}");
                    Console.WriteLine(" connstr {name}   returns connection string with name {name}");
                    return 0;
                }

                var conf = ConfigFactory.FromEnvJson(
                    applicationBasePath: Directory.GetCurrentDirectory(),
                    addEnvVariables: false,
                    environment: env);


                if (cmd == "get")
                {
                    if (path == null)
                    {
                        var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(conf.AsDictionaryPlain());
                        Console.WriteLine(serialized);
                    }
                    else
                    {
                        var val = conf.Get(path);
                        Console.WriteLine(val);
                    }

                    return 0;
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
    }
}

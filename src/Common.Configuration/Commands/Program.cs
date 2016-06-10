using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Common.Configuration.Commands
{
    public class Program
    {
        

        public static void Main(string[] args)
        {
            //Console.WriteLine($"args={string.Join(" ", args)}");

            string env = null;
            string cmd = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (i == 0)
                {
                    cmd = args[i];
                    continue;
                }
                if (args[i].Equals("--env", StringComparison.InvariantCultureIgnoreCase) ||
                    args[i].Equals("-env", StringComparison.InvariantCultureIgnoreCase))
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
                throw new ArgumentException("expected a command, like 'get'");
            }

            var conf = ConfigFactory.FromEnvJson(
                applicationBasePath: Directory.GetCurrentDirectory(),
                addEnvVariables: false,
                environment: env);


            if (cmd == "get")
            {
                var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(conf.AsDictionaryPlain());
                Console.WriteLine(serialized);

                return;
            }
        }
    }
}

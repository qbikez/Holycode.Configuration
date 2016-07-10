using System;
using System.Collections.Generic;
using System.Deployment.Internal;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using System.Threading;
using Microsoft.AspNet.Hosting;

namespace Holycode.Configuration.Tests.dnx
{
    public class Program
    {
        private Xunit.Runner.Dnx.Program runner;

        public Program(IApplicationEnvironment env, IServiceProvider svc)
        {
            runner = new Xunit.Runner.Dnx.Program(svc);
        }

        
        //public static void Main(string[] args) {
        //    Console.WriteLine("hello from tests");
        //}

        public void Main(string[] args)
        {
            Run(args);
        }

        public void Run(string[] args)
        {
            Console.WriteLine("Running tests");


            var myargs = args.Where(a => a.StartsWith("-p:"));
            var xunitargs = args.Where(a => !a.StartsWith("-p:"));


            Console.WriteLine($"current directory={System.IO.Directory.GetCurrentDirectory()}");

            try
            {
                runner.Main(xunitargs.ToArray());
            }
            finally
            {
                Console.WriteLine("exiting.");
            }
        }
    }
}

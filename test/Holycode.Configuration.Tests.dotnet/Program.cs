using System;
using System.Linq;

namespace Holycode.Configuration.Tests.dotnet
{
    public class Program
    {


        public static int Main(string[] args)
        {
            if (args.Contains("--debug")) {
                System.Diagnostics.Debugger.Launch();
                args = args.Where(a => a != "--debug").ToArray();                
            }
            Console.WriteLine("starting Xunit tests");
            if (args.Length == 0) {
                args = new [] { "HolyCode.Configuration.Tests.dotnet.exe" };
            }
            using (var program = new  Xunit.Runner.DotNet.Program()) {
                return program.Run(args);
            }
        }
    }
}

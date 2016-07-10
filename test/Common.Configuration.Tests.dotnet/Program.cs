using System;

namespace ConsoleApplication
{
    public class Program
    {


        public static int Main(string[] args)
        {
            Console.WriteLine("starting Xunit tests");
            using (var program = new  Xunit.Runner.DotNet.Program()) {
                return program.Run(args);
            }
        }
    }
}

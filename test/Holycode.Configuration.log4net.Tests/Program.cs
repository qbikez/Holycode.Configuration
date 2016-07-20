using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holycode.Configuration.log4net.Tests
{
    public class Program
    {
        public static void StartWebServer()
        {
            var host = new WebHostBuilder()
                   //.UseIISIntegration()
                   .UseKestrel(o =>
                   {                      
                   })
                   //.UseUrls(serverUrls.ToArray())
                   //.UseWebRoot("wwwroot")
                   //.UseContentRoot(Directory.GetCurrentDirectory())
                   .UseStartup<Startup>()
                   .Build();

            host.Run();
        }
    }
}

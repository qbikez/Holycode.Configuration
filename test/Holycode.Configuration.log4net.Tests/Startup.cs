using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using log4net;

namespace Holycode.Configuration.log4net.Tests
{
    internal class Startup 
    {
        public Startup()
        {
            
        }

        public void ConfigureServices(IServiceCollection services)
        {
            
        }

        public void Configure(IApplicationBuilder app)
        {
            var log = LogManager.GetLogger(typeof(Startup));
            
        }
    }
}
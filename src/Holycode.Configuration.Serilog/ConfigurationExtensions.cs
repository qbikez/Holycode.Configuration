using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Configuration
{
    public static class ConfigurationExtensions
    {
        public static void UseSerilog(this IConfiguration cfg, string appname = null, string baseDir = null)
        {
            Holycode.Configuration.Serilog.SerilogConfiguration.ConfigureSerilog(cfg, appname, baseDir);

        }
    }
}

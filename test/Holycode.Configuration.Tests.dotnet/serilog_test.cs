using Holycode.Configuration.Serilog;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace common.configuration.tests
{
    public class serilog_test
    {
        [Fact]
        public void manual_sinks_graylog()
        {
            var url = "net.udp://localhost:12201";
            var configuration = new ConfigurationBuilder().Build();
            var builder = new SerilogConfiguration(configuration, "test", ".");

            // default docker config
            builder.UseGraylog(url);

            Serilog.Log.Logger = builder.CreateLogger();

            Serilog.Log.Information($"testing graylog from .NET using url {url} at {DateTime.Now}");

            Serilog.Log.CloseAndFlush();

            Console.WriteLine($"graylog message sent to {url}");
            // aaand... check graylog!
        }
    }
}

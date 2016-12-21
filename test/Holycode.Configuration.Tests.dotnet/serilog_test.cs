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
            var ms = new Random().Next() % 1000;
            Serilog.Log.Information("generated time: {@time}", new { time = ms });

            Serilog.Log.CloseAndFlush();

            Console.WriteLine($"graylog message sent to {url}");
            // aaand... check graylog!
        }

        [Fact]
        public void log4net_with_seriog_file_sink()
        {
            var log = log4net.LogManager.GetLogger("root");
            log4net.Config.BasicConfigurator.Configure();

            var configuration = new ConfigurationBuilder().Build();
            var builder = new SerilogConfiguration(configuration, "test", ".");
            builder.UseFileSink();
            Serilog.Log.Logger = builder.CreateLogger();

            Log4net.Appender.Serilog.Configuration.Configure();

            try
            {
                InnerThrowing();
            } catch (Exception ex)
            {
                // stack trace points to rethrow statement and cointains wrapped inner exception
                log4net.LogManager.GetLogger("root").Error("caugth exception", ex);
            }

            Serilog.Log.CloseAndFlush();
        }

        private void InnerThrowing()
        {
            try
            {
                try
                {
                    throw new Exception("this is most inner exception");
                }
                catch(Exception ex)
                {                    
                    // stack trace points to original exception
                    log4net.LogManager.GetLogger("root").Error("rethrowing exception", ex);
                    throw;
                }
            } catch (Exception ex)
            {
                /// stack trace ponts to rethrow statement
                log4net.LogManager.GetLogger("root").Error("wrapping exception", ex);
                throw new Exception("this is inner exception wrapper", ex);
            }
        }
    }
}

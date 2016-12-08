using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Serilog.Sinks.Elasticsearch;
using Serilog.Sinks.RollingFileAlternate;
using Holycode.Configuration.Serilog.Filters;
using Holycode.Configuration.Serilog.Enrichers;

namespace Holycode.Configuration.Serilog
{
    public class SerilogConfiguration
    {
        private static LoggingLevelSwitch logLevel = new LoggingLevelSwitch(LogEventLevel.Warning);
        static IpFilter ipfilter;

      

        public static LoggerConfiguration LoggerConfiguration(IConfiguration configuration, string appname = null, string baseDir = null, Action<ElasticsearchSinkOptions> configureElastic = null)
        {
            logLevel.MinimumLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), configuration["Logging:LogLevel:Default"] ?? "Debug");
            ipfilter = new IpFilter(logLevel, configuration["Logging:Filters:Ip"]);

            var env = configuration.EnvironmentName() ?? "env";
            var prefix = "x";
            appname = appname ?? configuration["Logging:Domain"] ?? "some-app";
            var splits = appname.Split('/');
            if (splits.Length > 1)
            {
                prefix = splits[0];
                appname = splits[1];
            }

            var isAzureWebsite = configuration["WEBSITE_SITE_NAME"] != null;

            var logbuilder = new LoggerConfiguration()
                        //.ReadFrom.Configuration(configuration)
                        .MinimumLevel.Debug()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .MinimumLevel.Override("System", LogEventLevel.Warning)
                        .Enrich.FromLogContext()
                        .Enrich.With(new ThreadIdEnricher())
                        .Enrich.With(new StaticPropertiesEnricher(new System.Collections.Generic.Dictionary<string, object>()
                        {
                            { "domain", appname },
                            { "env", env },
                            { "MachineName", Environment.GetEnvironmentVariable("COMPUTERNAME") },
                            { "prefix", prefix }
                        }))
                        .Filter.With(ipfilter);
            //.Filter.ByExcluding(ev => ExcludeLogByContext(ev, "Microsoft", LogEventLevel.Warning))                            
            //.WriteTo.Logger(inner => 
            //    inner.WriteTo.LiterateConsole(
            //    outputTemplate: "[{Timestamp:HH:mm:ss} {Level} {ThreadId}] [{SourceContext}] {Message}{NewLine}{Exception}"))


            var fileSink = configuration["Logging:Sinks:File"];
            if (fileSink != "False")
            {
                if (baseDir == null)
                {
                    baseDir = ".";
                }
                var logDir = isAzureWebsite ? $"{baseDir}/../../LogFiles/application" : $"{baseDir}/log";
                logbuilder = logbuilder.WriteTo.Logger(inner =>
                    inner
                        .Filter.With(new LogContextFilter("SerilogWeb.Classic.ApplicationLifecycleModule", LogEventLevel.Warning))
                        .WriteTo.RollingFileAlternate($"{logDir}",
                        prefix: $"{appname}-{env}-{System.Diagnostics.Process.GetCurrentProcess().Id}",
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level} {ThreadId}] [{SourceContext}] ({ClientIp}) {Message}{NewLine}{Exception}",
                        retainedFileCountLimit: 100, fileSizeLimitBytes: (5 << 20)
                    )
                );
            }
            var elasticSink = configuration["Logging:Sinks:Elastic"] ?? configuration["Logging:Sinks:Elastic:ConnectionString"];
            if (elasticSink != null)
            {
                logbuilder = logbuilder.WriteTo.Logger(inner =>
                {
                    var elasticOpts = new ElasticsearchSinkOptions(new Uri(elasticSink ?? "http://localhost:9200"))
                    {
                        IndexFormat = $"logstash-{prefix}-{env}-{appname}-{{0:yyyy.MM.dd}}"
                    };
                    if (configureElastic != null) configureElastic(elasticOpts);
                    inner.WriteTo.Elasticsearch(elasticOpts);
                });
            }

            ChangeToken.OnChange(
                 () => configuration.GetReloadToken(),
                 () =>
                 {
                     Log.Logger.Information($"detected configuration change. setting log level to {configuration["Logging:LogLevel:Default"]}");
                     logLevel.MinimumLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), configuration["Logging:LogLevel:Default"]);
                     ipfilter.Ip = configuration["Logging:Filters:Ip"];
                 }
            );

            return logbuilder;
        }

        public static void ConfigureSerilog(IConfiguration configuration, string appname = null, string baseDir = null)
        {
            var logbuilder = LoggerConfiguration(configuration, appname, baseDir);
            Log.Logger = logbuilder.CreateLogger();
        }

        // public static void ConfigureSerilog(IConfiguration configuration, IHttpContextAccessor ctxAccessor)
        // {
        //     var logbuilder = LoggerConfiguration(configuration);
        //     logbuilder = logbuilder.Enrich.With(new ClientIpEnricher(ctxAccessor));

        //     Log.Logger = logbuilder.CreateLogger();
        // }
    }
}

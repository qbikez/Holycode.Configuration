using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using Serilog.Sinks.Elasticsearch;

namespace Holycode.Configuration.Serilog
{
    public class SerilogConfiguration
    {
        private static LoggingLevelSwitch logLevel = new LoggingLevelSwitch(LogEventLevel.Warning);
        static IpFilter ipfilter;

        private static bool ExcludeLogByContext(LogEvent ev, string name, LogEventLevel minLevel)
        {
            LogEventPropertyValue ctx;
            if (ev.Properties.TryGetValue("SourceContext", out ctx))
            {
                return ((ScalarValue)ctx).Value.ToString().StartsWith(name) && ev.Level < minLevel;
            }
            return false;
        }

        public static LoggerConfiguration LoggerConfiguration(IConfiguration configuration, string appname = null, string baseDir = null)
        {
            logLevel.MinimumLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), configuration["Logging:LogLevel:Default"]);
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
                        .Enrich.With(new LoggerDomainEnricher(appname))
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
                    inner.WriteTo.RollingFile($"{logDir}/{appname}-{env}.log",
                        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level} {ThreadId}] [{SourceContext}] ({ClientIp}) {Message}{NewLine}{Exception}",
                        retainedFileCountLimit: 100, fileSizeLimitBytes: (5 << 20)
                    )
                );
            }
            var elasticSink = configuration["Logging:Sinks:Elastic"];
            if (elasticSink != null)
            {
                logbuilder = logbuilder.WriteTo.Logger(inner =>
                                inner.WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(elasticSink ?? "http://localhost:9200"))
                                {
                                    IndexFormat = $"logstash-{prefix}-{env}-{appname}-{{0:yyyy.MM.dd}}"                                    
                                }));
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

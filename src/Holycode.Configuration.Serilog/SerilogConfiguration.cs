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
    public partial class SerilogConfiguration : LoggerConfiguration
    {
        private static LoggingLevelSwitch logLevel = new LoggingLevelSwitch(LogEventLevel.Warning);
        static IpFilter ipfilter;

        private readonly string baseDir = null;
        private readonly string appname;
        private readonly IConfiguration configuration;
        private readonly string prefix = "x";
        private readonly bool isAzureWebsite;
        private readonly string env;

        public SerilogConfiguration(IConfiguration configuration, string appname, string baseDir = null)
        {
            this.configuration = configuration;

            if (baseDir == null) baseDir = ".";
            this.baseDir = baseDir;

            appname = appname ?? configuration["Logging:Domain"] ?? "some-app";
            var splits = appname.Split('/');
            if (splits.Length > 1)
            {
                prefix = splits[0];
                appname = splits[1];
            }
            this.appname = appname;

            var isAzureWebsite = configuration["WEBSITE_SITE_NAME"] != null;

            env = configuration.EnvironmentName() ?? "env";

            Initialize();
        }

        private string LogDir => isAzureWebsite ? $"{baseDir}/../../LogFiles/application" : $"{baseDir}/log";


        

        private void Initialize()
        {
            logLevel.MinimumLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), configuration["Logging:LogLevel:Default"] ?? "Debug");
            ipfilter = new IpFilter(logLevel, configuration["Logging:Filters:Ip"]);

            var logBuilder = this
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


            ChangeToken.OnChange(
                 () => configuration.GetReloadToken(),
                 () =>
                 {
                     Log.Logger.Information($"detected configuration change. setting log level to {configuration["Logging:LogLevel:Default"]}");
                     logLevel.MinimumLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), configuration["Logging:LogLevel:Default"]);
                     ipfilter.Ip = configuration["Logging:Filters:Ip"];
                 }
            );
        }

        public LoggerConfiguration ConfigureSinks(Action<ElasticsearchSinkOptions> configureElastic = null)
        {
            var fileSink = configuration["Logging:Sinks:File"];
            if (fileSink != "False")
            {
                UseFileSink();
            }
            var elasticSink = configuration["Logging:Sinks:Elastic"] ?? configuration["Logging:Sinks:Elastic:ConnectionString"];
            if (elasticSink != null)
            {
                UseElasticSink(configureElastic);
            }

            return this;
        }


        public static LoggerConfiguration LoggerConfiguration(IConfiguration configuration, string appname = null, string baseDir = null, Action<ElasticsearchSinkOptions> configureElastic = null)
        {
            var builder = new SerilogConfiguration(configuration, appname, baseDir);
            builder.ConfigureSinks(configureElastic);

            return builder;

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


    public static class SerilogConfigurationExtensions
    {

    }
}

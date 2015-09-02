using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using log4net;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Framework.ConfigurationModel
{
    public static class LogConfigExtensions
    {
        public static IConfiguration ConfigureLog4net(this IConfiguration config, string appName, ILog log = null, string logRootPath = null)
        {
            log = log ?? LogManager.GetLogger("root");
            var env = config.Get("application:env") ?? config.Get("ASPNET_ENV") ?? "Development";

            var section = config.GetSubKeys("log4net.appenders");

            var solrConnectionString = config.GetConnectionStringValue("log4net:appenders:solr");
            var solrEnabled = (config.GetNullable<bool>("log4net:appenders:solr:enabled") ?? true) && solrConnectionString != null;
            if (solrEnabled)
            {
                log.AddSolrLog(options =>
                {
                    options.SolrUrl = solrConnectionString;
                    options.DebugLog = null;
                }, domain: appName);
            }

            var logFilename = config.GetConnectionStringValue("log4net:appenders:file");
            var fileEnabled = (config.GetNullable<bool>("log4net:appenders:file:enabled") ?? true);
            if (fileEnabled)
            {
                logRootPath = logRootPath ??
                              config.Get("application:wwwroot") ?? config.Get("application:basePath") ?? ".";
                logFilename = logFilename ?? "log/{appName}-{env}.log";
                var logfile = $"{logRootPath}/{logFilename}";
                log.AddFileAppender(logfile,
                    config: appender =>
                    {
                    });
            }

            var traceEnabled = config.GetNullable<bool>("log4net:appenders:trace:enabled") ?? true;
            if (traceEnabled && Debugger.IsAttached)
            {
                log.AddTraceAppender();
            }

            var consoleEnabled = config.GetNullable<bool>("log4net:appenders:console:enabled") ?? true;
            if (consoleEnabled)
            {
                log.AddConsoleAppenderColored();
            }

            return config;
        }
    }
}

namespace log4net
{
    public static class LogConfigExtensions
    {
        public static ILog Configure(this ILog log, IConfiguration config, string appName)
        {
            config.ConfigureLog4net(appName, log: log);
            return log;
        }
    }
}
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
        public static IConfiguration ConfigureLog4net(this IConfiguration config, string appName, ILog log = null, string logRootPath = null, bool internalDebug = false)
        {
            if (internalDebug)
            {
                log4net.Util.LogLog.InternalDebugging = true;
            }
            log = log ?? LogManager.GetLogger("root");
            var level = config.Get("log4net:level") ?? "Debug";
            log.SetLevel(level);
            
            var env = config.Get("application:env") ?? config.Get("ASPNET_ENV") ?? "Development";

            var section = config.GetSubKeys("log4net.appenders");

            ConfigureSolrLog(config, appName, log);

            ConfigureFileLog(config, appName, env, logRootPath, log);

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

        private static void ConfigureSolrLog(IConfiguration config, string appName, ILog log)
        {
            var solrConnectionString = config.GetConnectionStringValue("log4net:appenders:solr");
            var solrEnabled = (config.GetNullable<bool>("log4net:appenders:solr:enabled") ?? true) &&
                              solrConnectionString != null;
            if (solrConnectionString?.Equals("False") ?? false)
            {
                solrConnectionString = null;
                solrEnabled = false;
            }
            if (solrEnabled)
            {
                log.AddSolrLog(options =>
                {
                    options.SolrUrl = solrConnectionString;
                    options.DebugLog = null;
                },
                    domain: appName);
            }
        }

        private static void ConfigureFileLog(IConfiguration config, string appName, string env, string logRootPath, ILog log)
        {
            var logFilename = config.GetConnectionStringValue("log4net:appenders:file");
            var fileEnabled = (config.GetNullable<bool>("log4net:appenders:file:enabled") ?? true);

            if (logFilename?.Equals("True") ?? false)
            {
                logFilename = null;
                fileEnabled = true;
            }
            if (logFilename?.Equals("False") ?? false)
            {
                fileEnabled = false;
            }

            if (fileEnabled)
            {
                logRootPath = logRootPath ??
                              config.Get("application:wwwroot") ?? config.Get("application:basePath") ?? ".";
                logFilename = logFilename ?? $"log\\{appName}-{env}.log";
                var logfile = $"{logRootPath}\\{logFilename}";
                log.AddFileAppender(logfile);
                log.DebugFormat("configured file appender");
            }
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
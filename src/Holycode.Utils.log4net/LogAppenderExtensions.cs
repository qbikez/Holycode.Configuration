using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace log4net
{
    public static class LogAppenderExtensions
    {

        public static void AddTapAppender(this ILog log)
        {
            log.AddAppender(LogManagerTools.CreateTapAppender());
        }

        public static void AddTraceAppender(this ILog log)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "TraceAppender"))
                return;
            LogManagerTools.AddTraceAppender();
        }

        public static void AddFileAppender(this ILog log, string filename, string appenderName = "RollingFileAppender", bool minimalLock = true,
            Action<RollingFileAppender> config = null)
        {
            var appender = LogManagerTools.CreateFileAppender(filename, appenderName, minimalLock, config);
            log.AddAppender(appender);
        }

        public static void AddConsoleAppender(this ILog log)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "ConsoleAppender"))
                return;
            LogManagerTools.AddConsoleAppender();
        }

        public static void AddConsoleAppenderColored(this ILog log)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "ConsoleAppender"))
                return;
            LogManagerTools.AddConsoleAppenderColored();
        }

        public static void AddTerminalAppenderColored(this ILog log)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "TerminalAppender"))
                return;
            LogManagerTools.AddTerminalAppenderColored();
        }


        public static void AddSmtpAppender(this ILog log, string sendTo, string programName = "", Level levelMin = null)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "SmtpAppender"))
                return;
            LogManagerTools.AddSmtpAppender(sendTo, programName);
        }


        public static void AddCallbackAppender(this ILog log, Action<string, LoggingEvent> callback)
        {
            var layout = LogManagerTools.CreateLayout(LogManagerTools.DefaultLayoutPattern);
            var appender = new Appender.CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
            log.AddAppender(appender);
        }

        public static void AddCallbackAppender(this ILog log, Action<string> callback)
        {
            var layout = LogManagerTools.CreateLayout(LogManagerTools.DefaultLayoutPattern);
            var appender = new Appender.CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
            log.AddAppender(appender);
        }
    }
}

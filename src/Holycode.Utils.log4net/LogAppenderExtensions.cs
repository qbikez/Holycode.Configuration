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

        public static void AddTapAppender(this ILog log) => log.AddAppender(AppenderFactory.CreateTapAppender());

        public static void AddFileAppender(this ILog log,
            string filename, string appenderName = "RollingFileAppender", bool minimalLock = true,
            Action<RollingFileAppender> config = null)
            => log.AddAppender(AppenderFactory.CreateFileAppender(filename, appenderName, minimalLock, config));

 
        public static void AddCallbackAppender(this ILog log, Action<string, LoggingEvent> callback)
            => log.AddAppender(AppenderFactory.CreateCallbackAppender(callback));

        public static void AddCallbackAppender(this ILog log, Action<string> callback)
            => log.AddAppender(AppenderFactory.CreateCallbackAppender(callback));
    }
}

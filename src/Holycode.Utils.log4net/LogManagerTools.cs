using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net.Filter;
using log4net.Layout;
using log4net.Util;
using log4net.Repository.Hierarchy;

namespace log4net
{
    public static class LogManagerTools
    {
        public static void FlushBuffers()
        {

            ILoggerRepository rep = LogManager.GetRepository();


            foreach (IAppender appender in rep.GetAppenders())
            {
                var buffered = appender as BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                }
            }
        }

        public static void AddAppender(AppenderSkeleton appender)
        {
            appender.ActivateOptions();
            AddAppender((IAppender)appender);
        }

        public static void AddAppender(IAppender appender) => AddGlobalAppender(appender);

        internal static void AddGlobalAppender(IAppender appender) => AddGlobalAppender(LogManager.GetRepository(), appender);

        internal static void AddGlobalAppender(ILoggerRepository repository, IAppender appender)
        {
            if (repository.GetAppenders().Any(a => a.Name == appender.Name)) return;
            log4net.Config.BasicConfigurator.Configure(repository, appender);
        }

        public static void AddFileAppender(string filename, string appenderName = "RollingFileAppender", bool minimalLock = true,
        Action<RollingFileAppender> config = null) 
            => AddAppender(AppenderFactory.CreateFileAppender(filename, appenderName, minimalLock: minimalLock, config: config));


        public static void AddCallbackAppender(Action<string, LoggingEvent> callback)
            => AddAppender(AppenderFactory.CreateCallbackAppender(callback));

        public static void AddCallbackAppender(Action<string> callback)
            => AddAppender(AppenderFactory.CreateCallbackAppender(callback));



        // Set the level for a named logger
        public static void SetLevel(string loggerName, Level level)
        {
            ILog log = LogManager.GetLogger(loggerName);
            var l = (Logger)log.Logger;

            l.Level = level;//l.Hierarchy.LevelMap[levelName];
        }
    }
}

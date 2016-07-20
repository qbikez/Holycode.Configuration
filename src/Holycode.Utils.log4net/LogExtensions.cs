using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using log4net.Config;

namespace log4net
{
    public static class LogExtensions
    {


        // Set the level for a named logger
        public static void SetLevel(this ILog log, Level level) => ((Logger)log.Logger).Level = level;


        // Set the level for a named logger
        public static void SetLevel(this ILog log, string levelName) => ((Logger)log.Logger).SetLevel(levelName);

        public static void SetLevel(this Logger l, string levelName) => l.Level = l.Hierarchy.LevelMap[levelName];

        public static ILog SetAdditivity(this ILog log, bool val)
        {
            ((Logger)log.Logger).Additivity = val;
            return log;
        }


        // Add an appender to a logger
        public static void AddAppender(this ILog log, AppenderSkeleton appender)
        {
            appender.ActivateOptions();
            log.AddAppender((IAppender)appender);
        }


        // Add an appender to a logger

        public static void AddAppender(this ILog log, IAppender appender)
        {
            Logger l = (Logger)log.Logger;

            if (l.GetAppender(appender.Name) != null)
                return;

            if (!l.Repository.Configured)
            {
                BasicConfigurator.Configure();
            }
            if (l.Name == "root")
            {
                LogManagerTools.AddGlobalAppender(appender);
            }
            else
            {
                l.AddAppender(appender);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="repository"></param>
        /// <param name="level">level of inner exceptions to show. default = null. 0 - don't show inner exceptions, 1 - show only the first inner exception, etc.</param>
        /// <param name="showStackTrace">should the stacktrace be printed</param>
        public static void AddExceptionRenderer(this ILoggerRepository repository, int? level = null, bool showStackTrace = true)
        {
            repository.RendererMap.Put(typeof(Exception), new ExceptionRenderer()
            {
                InnerExceptionLevel = level,
                ShowStackTrace = showStackTrace
            });
        }
    }
}

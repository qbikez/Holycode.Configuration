using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting;
using System.Text;
using log4net.Filter;
using log4net.Layout;
using log4net.Util;
using log4net.Repository.Hierarchy;

namespace log4net
{
    public static class LogManagerTools
    {
        public static string DefaultLayoutPattern =
            @"%date [%-5.5thread] %-5level: [ %-30.30logger ] %-100message%newline";
        public static string DefaultTracceLayoutPattern =
                    @"%date [%-2.2thread] %-5level: [%-10.10logger] %-100message";
        public static string DefaultConsoleLayoutPattern = @"%date [%-2thread] %-5level: [%-10.10logger] %message%newline";
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


        public static void AddTraceAppender()
        {
            var layout = new log4net.Layout.PatternLayout(DefaultTracceLayoutPattern);
            var appender = new log4net.Appender.AzureTraceAppender()
            {
                Layout = layout,
                Name = "TraceAppender",
                ImmediateFlush = true,
            };
            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static void AddTapAppender()
        {
            var layout = new log4net.Layout.PatternLayout(DefaultLayoutPattern);
            var appender = new log4net.Appender.LogTapAppender()
            {
                Name = "LogTapAppender"
            };

            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static void AddConsoleAppender()
        {
            var layout = new log4net.Layout.PatternLayout(DefaultConsoleLayoutPattern);
            var appender = new log4net.Appender.ConsoleAppender()
            {
                Layout = layout,
                Threshold = Level.Debug,
                Name = "ConsoleAppender",
            };
            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);
        }
        public static void AddConsoleAppenderColored()
        {
            var layout = new log4net.Layout.PatternLayout(DefaultConsoleLayoutPattern);
            var appender = new log4net.Appender.ColoredConsoleAppender()
            {
                Layout = layout,
                Threshold = Level.Debug,
                Name = "ConsoleAppender",
            };
            appender.AddMapping(new ColoredConsoleAppender.LevelColors() { Level = Level.Debug, ForeColor = ColoredConsoleAppender.Colors.Green });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors() { Level = Level.Info, ForeColor = ColoredConsoleAppender.Colors.White });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors() { Level = Level.Warn, ForeColor = ColoredConsoleAppender.Colors.Yellow });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors() { Level = Level.Error, ForeColor = ColoredConsoleAppender.Colors.Red });

            appender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static void AddTerminalAppenderColored()
        {
            var layout = new log4net.Layout.PatternLayout(LogManagerTools.DefaultConsoleLayoutPattern);
            var appender = new log4net.Appender.AnsiColorTerminalAppender()
            {
                Layout = layout,
                Threshold = Level.Debug,
                Name = "TerminalAppender",
            };
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors() { Level = Level.Debug, ForeColor = AnsiColorTerminalAppender.AnsiColor.Green });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors() { Level = Level.Info, ForeColor = AnsiColorTerminalAppender.AnsiColor.White });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors() { Level = Level.Warn, ForeColor = AnsiColorTerminalAppender.AnsiColor.Yellow });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors() { Level = Level.Error, ForeColor = AnsiColorTerminalAppender.AnsiColor.Red });

            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static RollingFileAppender CreateFileAppender(string filename,
            string appenderName = "RollingFileAppender", 
            bool minimalLock = true,
            Action<RollingFileAppender> config = null)
        {
            var layout = new log4net.Layout.PatternLayout(DefaultLayoutPattern);
            var appender = new log4net.Appender.RollingFileAppender()
            {
                Layout = layout,
                Threshold = Level.Debug,
                ImmediateFlush = true,
                AppendToFile = true,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                PreserveLogFileNameExtension = true,
                File = filename,
                Name = appenderName,
                LockingModel = minimalLock ? (FileAppender.LockingModelBase)new FileAppender.MinimalLock() : new FileAppender.ExclusiveLock(),
                MaxSizeRollBackups = -1,
                CountDirection = 1,
                StaticLogFileName = true,
            };

            if (config != null)
                config(appender);
            appender.ActivateOptions();

            return appender;
        }

        public static void AddFileAppender(string filename, bool minimalLock = true,
            Action<RollingFileAppender> config = null)
        {
            var appender = CreateFileAppender(filename, minimalLock: minimalLock, config: config);            
            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static void AddSmtpAppender(string sendto, string programName = "", Level levelMin = null)
        {
            const string layout = "%newline[%property{log4net:HostName}]%newline%date [%thread] %-5level %logger [%property{Username}] - %message%newline%newline%newline";
            string subjectLayout = "[%property{log4net:HostName}] Error @ " + programName;

            var appender = new SmtpAppenderWithSubjectLayout()
            {
                Name = "SmtpAppender",
                SmtpHost = "legimi.home.pl",
                Port = 25,
                Authentication = SmtpAppender.SmtpAuthentication.Basic,
                Username = "crash@legimi.com",
                Password = "crash_ol_je",
                BufferSize = 512,
                Lossy = false,
                Layout = new PatternLayout(layout),
                SubjectLayout = new PatternLayout(subjectLayout),
                Evaluator = new LevelEvaluator(threshold: levelMin ?? Level.Error),
                To = sendto,
                From = "crash@legimi.com",
            };

            appender.AddFilter(new LevelRangeFilter()
            {
                AcceptOnMatch = true,
                LevelMin = levelMin ?? Level.Error
            });

            appender.ActivateOptions();

            log4net.Config.BasicConfigurator.Configure(appender);
        }

        public static void AddTapAppender(this ILog log)
        {
            if (log.Logger.Repository.GetAppenders().Any(a => a.Name == "LogTapAppender"))
                return;
            LogManagerTools.AddTapAppender();
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
            //if (log.Logger.Repository.GetAppenders().Any(a => a.Name == appenderName))
            //    return;
            var appender = CreateFileAppender(filename, appenderName, minimalLock, config);
            //if (log.Logger.Name == "root")
            //{
                LogManagerTools.AddFileAppender(filename, minimalLock, config);
            //}
            //else
            //{
            //    AddAppender(log, appender);
            //}
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



        // Set the level for a named logger
        public static void SetLevel(string loggerName, Level level)
        {
            ILog log = LogManager.GetLogger(loggerName);
            var l = (Logger)log.Logger;

            l.Level = level;//l.Hierarchy.LevelMap[levelName];
        }


        // Set the level for a named logger
        public static void SetLevel(this ILog log, Level level)
        {
            var l = (Logger)log.Logger;

            l.Level = level;//l.Hierarchy.LevelMap[levelName];
        }


        // Set the level for a named logger
        public static void SetLevel(this ILog log, string levelName)
        {
            var l = (Logger)log.Logger;

            l.SetLevel(levelName);
        }

        public static void SetLevel(this Logger l, string levelName)
        {
            l.Level = l.Hierarchy.LevelMap[levelName];
        }

        

        // Add an appender to a logger
        public static void AddAppender(this ILog log, AppenderSkeleton appender)
        {
            appender.ActivateOptions();
            log4net.Config.BasicConfigurator.Configure(appender);
        }

        // Add an appender to a logger
        public static void AddConfiguredAppender(string loggerName, IAppender appender)
        {
            ILog log = LogManager.GetLogger(loggerName);
            Logger l = (Logger)log.Logger;

            l.AddAppender(appender);
        }

        // Add an appender to a logger
        public static void AddConfiguredAppender(this ILog log, IAppender appender)
        {
            Logger l = (Logger)log.Logger;
            
            l.AddAppender(appender);
        }

        public static ILog SetAdditivity(this ILog log, bool val)
        {
            Logger l = (Logger)log.Logger;
            l.Additivity = val;
            return log;
        }

        //// Create a new file appender
        //public static IAppender CreateFileAppender(string name, string fileName)
        //{
        //    FileAppender appender = new
        //        FileAppender();
        //    appender.Name = name;
        //    appender.File = fileName;
        //    appender.AppendToFile = true;

        //    PatternLayout layout = new PatternLayout();
        //    layout.ConversionPattern = "%d [%t] %-5p %c [%x] - %m%n";
        //    layout.ActivateOptions();

        //    appender.Layout = layout;
        //    appender.ActivateOptions();

        //    return appender;
        //}

    }


}

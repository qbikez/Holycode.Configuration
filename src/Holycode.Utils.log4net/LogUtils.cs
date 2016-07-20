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


        public static void AddAppender(IAppender appender)
        {
            AddGlobalAppender(appender);
        }

        public static void AddAppender(AppenderSkeleton appender) {
            appender.ActivateOptions();
            AddAppender((IAppender)appender);
        }

        internal static void AddGlobalAppender(IAppender appender)
        {
            AddGlobalAppender(LogManager.GetRepository(), appender);
        }

        internal static void AddGlobalAppender(ILoggerRepository repository, IAppender appender)
        {
            if (repository.GetAppenders().Any(a => a.Name == appender.Name)) return;
            log4net.Config.BasicConfigurator.Configure(repository, appender);
        }

        public static void AddTraceAppender()
        {
            var layout = CreateLayout(DefaultTracceLayoutPattern);
            var appender = new log4net.Appender.AzureTraceAppender()
            {
                Layout = layout,
                Name = "TraceAppender",
                ImmediateFlush = true,
            };
            appender.ActivateOptions();
            
        }

        public static void AddTapAppender() => AddAppender(CreateTapAppender());
        

        internal static LogTapAppender CreateTapAppender()
        {
            var layout = CreateLayout(DefaultLayoutPattern);
            var appender = new log4net.Appender.LogTapAppender()
            {
                Name = "LogTapAppender"
            };
            return appender;
        }

        public static void AddConsoleAppender()
        {
            var layout = CreateLayout(DefaultConsoleLayoutPattern);
            var appender = new log4net.Appender.ConsoleAppender()
            {
                Layout = layout,
                Threshold = Level.Debug,
                Name = "ConsoleAppender",

            };
            appender.ActivateOptions();
            AddAppender(appender);
        }
        public static void AddConsoleAppenderColored()
        {
            var layout = CreateLayout(DefaultConsoleLayoutPattern);
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

            AddAppender(appender);
        }

        public static void AddTerminalAppenderColored()
        {
            var layout = CreateLayout(LogManagerTools.DefaultConsoleLayoutPattern);
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
            AddAppender(appender);
        }

        public static RollingFileAppender CreateFileAppender(string filename,
            string appenderName = "RollingFileAppender",
            bool minimalLock = true,
            Action<RollingFileAppender> config = null)
        {
            var layout = CreateLayout(DefaultLayoutPattern);
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

        

        public static void AddSmtpAppender(string sendto, string programName = "", Level levelMin = null)
        {
            const string layout = "%newline[%property{log4net:HostName}]%newline%date [%thread] %-5level %logger [%property{Username}] - %message%newline%newline%newline";
            string subjectLayout = "[%property{log4net:HostName}] Error @ " + programName;

            var appender = new SmtpAppenderWithSubjectLayout()
            {
                Name = "SmtpAppender",
                SmtpHost = "my.domain.com",
                Port = 25,
                Authentication = SmtpAppender.SmtpAuthentication.Basic,
                Username = "me@my.domain.com",
                Password = "secret",
                BufferSize = 512,
                Lossy = false,
                Layout = CreateLayout(layout),
                SubjectLayout = new PatternLayout(subjectLayout),
                Evaluator = new LevelEvaluator(threshold: levelMin ?? Level.Error),
                To = sendto,
                From = "me@my.domain.com",
            };

            appender.AddFilter(new LevelRangeFilter()
            {
                AcceptOnMatch = true,
                LevelMin = levelMin ?? Level.Error
            });

            appender.ActivateOptions();

            AddAppender(appender);
        }

        public static void AddFileAppender(string filename, string appenderName = "RollingFileAppender", bool minimalLock = true,
        Action<RollingFileAppender> config = null)
        {
            var appender = CreateFileAppender(filename, appenderName, minimalLock: minimalLock, config: config);
            AddAppender(appender);
        }




        public static void AddCallbackAppender(Action<string, LoggingEvent> callback)
        {
            var layout = CreateLayout(DefaultLayoutPattern);
            var appender = new log4net.Appender.CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
            AddAppender(appender);
        }

        public static void AddCallbackAppender(Action<string> callback)
        {
            var layout = CreateLayout(DefaultLayoutPattern);
            var appender = new log4net.Appender.CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
            AddAppender(appender);
        }



        // Set the level for a named logger
        public static void SetLevel(string loggerName, Level level)
        {
            ILog log = LogManager.GetLogger(loggerName);
            var l = (Logger)log.Logger;

            l.Level = level;//l.Hierarchy.LevelMap[levelName];
        }
 

        // Add an appender to a logger
        public static void AddConfiguredAppender(string loggerName, IAppender appender)
        {
            ILog log = LogManager.GetLogger(loggerName);            
            log.AddAppender(appender);
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

        internal static ILayout CreateLayout(string layout)
        {
            var l = new PatternLayout(layout) { IgnoresException = true };
            l.ActivateOptions();
            return l;
        }
    }
}

using System;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace log4net
{
    public static class AppenderFactory
    {
        public static string DefaultLayoutPattern =
            @"%date [%-5.5thread] %-5level: [ %-30.30logger ] %-100message%newline";

        public static string DefaultTracceLayoutPattern =
            @"%date [%-2.2thread] %-5level: [%-10.10logger] %-100message";

        public static string DefaultConsoleLayoutPattern = @"%date [%-2thread] %-5level: [%-10.10logger] %message%newline";

        internal static AzureTraceAppender CreateTraceAppender() => new AzureTraceAppender()
        {
            Layout = CreateLayout(DefaultTracceLayoutPattern),
            Name = "TraceAppender",
            ImmediateFlush = true,
        };

        internal static LogTapAppender CreateTapAppender() => new LogTapAppender()
        {
            Name = "LogTapAppender",
        };

        internal static ConsoleAppender CreateConsoleAppender() => new ConsoleAppender()
        {
            Layout = CreateLayout(DefaultConsoleLayoutPattern),
            Threshold = Level.Debug,
            Name = "ConsoleAppender",

        };

#if !CORECLR

        internal static AppenderSkeleton CreateConsoleAppenderColored()
        {
            var appender = new ColoredConsoleAppender()
            {
                Layout = CreateLayout(DefaultConsoleLayoutPattern),
                Threshold = Level.Debug,
                Name = "ConsoleAppender",
            };
            appender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Debug,
                ForeColor = ColoredConsoleAppender.Colors.Green
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Info,
                ForeColor = ColoredConsoleAppender.Colors.White
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Warn,
                ForeColor = ColoredConsoleAppender.Colors.Yellow
            });
            appender.AddMapping(new ColoredConsoleAppender.LevelColors()
            {
                Level = Level.Error,
                ForeColor = ColoredConsoleAppender.Colors.Red
            });
            return appender;
        }
#else
        internal static AppenderSkeleton CreateConsoleAppenderColored() => CreateConsoleAppender();
#endif
        internal static AnsiColorTerminalAppender CreateAnsiColorTerminalAppender()
        {
            var appender = new AnsiColorTerminalAppender()
            {
                Layout = CreateLayout(AppenderFactory.DefaultConsoleLayoutPattern),
                Threshold = Level.Debug,
                Name = "TerminalAppender",
            };
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors()
            {
                Level = Level.Debug,
                ForeColor = AnsiColorTerminalAppender.AnsiColor.Green
            });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors()
            {
                Level = Level.Info,
                ForeColor = AnsiColorTerminalAppender.AnsiColor.White
            });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors()
            {
                Level = Level.Warn,
                ForeColor = AnsiColorTerminalAppender.AnsiColor.Yellow
            });
            appender.AddMapping(new AnsiColorTerminalAppender.LevelColors()
            {
                Level = Level.Error,
                ForeColor = AnsiColorTerminalAppender.AnsiColor.Red
            });
            return appender;
        }

        public static RollingFileAppender CreateFileAppender(string filename,
            string appenderName = "RollingFileAppender",
            bool minimalLock = true,
            Action<RollingFileAppender> config = null)
        {
            var layout = CreateLayout(DefaultLayoutPattern);
            var appender = new RollingFileAppender()
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

            return appender;
        }

        internal static ILayout CreateLayout(string layout)
        {
            var l = new PatternLayout(layout) { IgnoresException = true };
            l.ActivateOptions();
            return l;
        }

#if !CORECLR
        internal static SmtpAppenderWithSubjectLayout CreateSmtpAppender(string sendto, string programName, Level levelMin)
        {
            const string layout =
                "%newline[%property{log4net:HostName}]%newline%date [%thread] %-5level %logger [%property{Username}] - %message%newline%newline%newline";
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
                Layout = AppenderFactory.CreateLayout(layout),
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
            return appender;
        }

#endif
        internal static CallbackAppender CreateCallbackAppender(Action<string> callback)
        {
            return new CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
        }

        internal static CallbackAppender CreateCallbackAppender(Action<string, LoggingEvent> callback)
        {
            return new Appender.CallbackAppender(callback)
            {
                Name = "CallbackAppender"
            };
        }
    }
}
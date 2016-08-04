using System;
using log4net.Appender;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;

namespace log4net
{
    public class AppenderFactory
    {
        public AppenderFactory Instance { get; } = new AppenderFactory();
        public static string DefaultLayoutPattern =
            @"%date [%-5.5thread] %-5level: [ %-30.30logger ] %-100message%newline";

        public static string DefaultTracceLayoutPattern =
            @"%date [%-2.2thread] %-5level: [%-10.10logger] %-100message";

        public static string DefaultConsoleLayoutPattern = @"%date [%-2thread] %-5level: [%-10.10logger] %message%newline";

     
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
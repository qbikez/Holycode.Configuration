using log4net.Appender;
using log4net.Core;

namespace log4net
{
    public static class ConsoleAppenderExensions
    {
        public static void AddConsoleAppender(this ILog log) => log.AddAppender(AppenderFactory.CreateConsoleAppender());

        public static void AddTerminalAppenderColored(this ILog log)
            => log.AddAppender(CreateAnsiColorTerminalAppender());

        #if CONSOLE_APPENDER   

        public static void AddConsoleAppenderColored(this ILog log) => log.AddAppender(CreateConsoleAppenderColored());


        internal static ColoredConsoleAppender CreateConsoleAppenderColored()
        {
            var appender = new ColoredConsoleAppender()
            {
                Layout = AppenderFactory.CreateLayout(AppenderFactory.DefaultConsoleLayoutPattern),
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

        #endif

        internal static AnsiColorTerminalAppender CreateAnsiColorTerminalAppender()
        {
            var appender = new AnsiColorTerminalAppender()
            {
                Layout = AppenderFactory.CreateLayout(AppenderFactory.DefaultConsoleLayoutPattern),
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

    }
}
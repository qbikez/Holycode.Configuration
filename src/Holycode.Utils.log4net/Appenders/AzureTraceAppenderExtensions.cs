#if !COREFX
using log4net.Appender;

namespace log4net
{
    public static class AzureTraceAppenderExtensions
    {
        public static void AddTraceAppender(this ILog log) => log.AddAppender(CreateTraceAppender());

        internal static AzureTraceAppender CreateTraceAppender(this AppenderFactory f) => CreateTraceAppender();

        private static AzureTraceAppender CreateTraceAppender() => new AzureTraceAppender()
        {
            Layout = AppenderFactory.CreateLayout(AppenderFactory.DefaultTracceLayoutPattern),
            Name = "TraceAppender",
            ImmediateFlush = true,
        };

    }
}
#endif
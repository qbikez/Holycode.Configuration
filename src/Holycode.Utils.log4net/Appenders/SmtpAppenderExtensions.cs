#if SMTP_APPENDER

using log4net.Core;
using log4net.Appender;

namespace log4net
{
    public static class SmtpAppenderExtensions
    {
        public static void AddSmtpAppender(this ILog log, string sendTo, string programName = "", Level levelMin = null)
            => log.AddAppender(CreateSmtpAppender(sendTo, programName, levelMin));
    
            
        internal static SmtpAppenderWithSubjectLayout CreateSmtpAppender(this AppenderFactory f, string sendto, string programName, Level levelMin) => CreateSmtpAppender(sendto, programName, levelMin);

        private static SmtpAppenderWithSubjectLayout CreateSmtpAppender(string sendto, string programName, Level levelMin)
        
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
    }
}

#endif


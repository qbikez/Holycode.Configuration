using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace log4net.Appender
{
    /// <summary>
    /// A special appender that appends to a StringBuilder when used with LogTapper
    /// <example>
    /// var sb = new StringBuilder();
    /// using (LogTapper.CaptureTo(sb))
    /// {
    ///     //do something that invokes log
    /// }
    /// var logstring = sb.ToString();
    /// </example>
    /// </summary>
    public class LogTapAppender : IAppender
    {
        public string Name { get; set; }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            var capture = ThreadContext.Properties["LogOutputTo"] as StringBuilder;
            if (capture == null)
                return;

            capture.AppendLine(DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " " + loggingEvent.Level.DisplayName.PadRight(6, ' ') + " " + loggingEvent.RenderedMessage);

            if (loggingEvent.ExceptionObject != null)
            {
                capture.Append(loggingEvent.ExceptionObject.ToString());
                capture.AppendLine();
            }
        }

        public void Close()
        {
        }
    }
}

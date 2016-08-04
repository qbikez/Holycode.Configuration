#if !COREFX
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using log4net.Core;

namespace log4net.Appender
{
    public class AzureTraceAppender : TraceAppender
    {
        protected override void Append(LoggingEvent loggingEvent)
        {
            var level = loggingEvent.Level;
            var message = RenderLoggingEvent(loggingEvent);

            if (level >= Level.Error)
                Trace.TraceError(message);
            else if (level >= Level.Warn)
                Trace.TraceWarning(message);
            else if (level >= Level.Info)
                Trace.TraceInformation(message);
            else
                Trace.WriteLine(message);

            if (ImmediateFlush)
                Trace.Flush();
        }
    }
}
#endif
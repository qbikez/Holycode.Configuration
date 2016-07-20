using log4net.Appender;
using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace log4net.Appender
{
    public class CallbackAppender : IAppender
    {
        private readonly Action<string, LoggingEvent> _callback;
        public string Name { get; set; }

        public CallbackAppender(Action<string> callback)
        {
            _callback = (s, e) => callback(s);
        }

        public CallbackAppender(Action<string, LoggingEvent> callback)
        {
            _callback = callback;
        }

        public void DoAppend(LoggingEvent loggingEvent)
        {
            _callback(loggingEvent.RenderedMessage, loggingEvent);
            

            if (loggingEvent.ExceptionObject != null)
            {
                _callback(loggingEvent.ExceptionObject.ToString(), loggingEvent);
            }
        }

        public void Close()
        {
        }
    }
}

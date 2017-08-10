using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace Holycode.Extensions.log4net
{
    public class Log4NetTraceListener : TraceListener
    {
        ILog _logger = LogManager.GetLogger(typeof(TraceListener));

        public override void Write(string message)
        {
            _logger.Info(message);
        }

        public override void WriteLine(string message)
        {
            _logger.Info(message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace log4net
{
    public class LogTapper : IDisposable
    {
        LogTapper(StringBuilder builder)
        {
            ThreadContext.Properties["LogOutputTo"] = builder;
        }

        public static IDisposable CaptureTo(StringBuilder builder)
        {
            return new LogTapper(builder);
        }

        public void Dispose()
        {
            ThreadContext.Properties.Remove("LogOutputTo");
        }
    }
}

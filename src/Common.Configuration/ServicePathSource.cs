using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    public class ServicePathSource
    {
        public string ServiceName;
        public string Source;
        public string AdditionalInfo;
        public ServicePathSource()
        {
            
        }
        public ServicePathSource(string serviceName, string source)
        {
            this.ServiceName = serviceName;
            this.Source = source;
        }
        public ServicePathSource(string serviceName, string source, string additionalInfo) : this(serviceName, source)
        {
            this.AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"{ServiceName}: '{Source}' ({AdditionalInfo})";
        }
    }
}

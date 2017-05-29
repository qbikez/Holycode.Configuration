using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    public class ConfigPathSource
    {
        public string Source;
        public string AdditionalInfo;
        public int Priority;

        public string Directory => System.IO.Path.GetDirectoryName(Source);

        public ConfigPathSource()
        {
            
        }
        public ConfigPathSource(string source)
        {
            this.Source = source;
        }
        public  ConfigPathSource(string source, string additionalInfo) : this( source)
        {
            this.AdditionalInfo = additionalInfo;
        }

        public override string ToString()
        {
            return $"'{Source}' ({AdditionalInfo})";
        }
    }
}

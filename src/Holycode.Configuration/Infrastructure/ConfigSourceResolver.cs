using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.FileProviders;

namespace Holycode.Configuration
{
    public interface IConfigSourceConvention {
        IEnumerable<IConfigurationSource> GetConfigSources();
    }

  
    public class ConfigSourceResolver : IConfigSourceConvention
    {
        private IEnumerable<IConfigSourceConvention> Conventions;

        public bool StopOnFirstMatch { get; private set; }
    
        public ConfigSourceResolver(IEnumerable<IConfigSourceConvention> conventions, bool stopOnFirstMatch = false)
        {
            this.Conventions = conventions;
            this.StopOnFirstMatch = stopOnFirstMatch;
        }


        public IEnumerable<IConfigurationSource> GetConfigSources()
        {
            List<IConfigurationSource> sources = new List<IConfigurationSource>();
            foreach(var c in Conventions) {
                var cs = c.GetConfigSources();
                if (cs?.Any() ?? false) { 
                    sources.AddRange(cs);
                    if (StopOnFirstMatch) break;
                }
            }

            return sources;
        }
    }
}

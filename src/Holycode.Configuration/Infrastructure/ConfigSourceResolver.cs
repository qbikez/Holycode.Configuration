﻿using System;
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

    
        public ConfigSourceResolver(IEnumerable<IConfigSourceConvention> conventions)
        {
            this.Conventions = conventions;
        }
      
        public IEnumerable<IConfigurationSource> GetConfigSources()
        {
            List<IConfigurationSource> sources = new List<IConfigurationSource>();
            foreach(var c in Conventions) {
                var cs = c.GetConfigSources();
                if (cs != null) sources.AddRange(cs);
            }

            return sources;
        }
    }
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    public interface IConfigFileConvention
    {
        IEnumerable<ConfigPathSource> GetConfigFiles(string directory);
    }

    public class EnvJsonConvention : IConfigFileConvention
    {
        public IEnumerable<ConfigPathSource> GetConfigFiles(string path)
        {
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            int levelUp = 0; // 0 for a path to a file and 1 for directory          

            for (string dir = path; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir), levelUp++)
            {
                var dirname = Path.GetFileName(dir);

                if (System.IO.Directory.Exists(dir))
                {
                    var found = GetConfigFilesInDir(dir);
                    if (found != null && found.Any()) {
                        names.AddRange(found);
                        break;
                    }
                }
            }

            return names;
        }
        private  IEnumerable<ConfigPathSource> GetConfigFilesInDir(string directory)
        {
            // stop at first env.json
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            var dirname = Path.GetFileName(directory);
            // search for env.json                
            var files = System.IO.Directory.GetFiles(directory, "env.json");
            if (files.Length > 0)
            {
                foreach (var f in files)
                {
                    names.Add(new ConfigPathSource(f, $"[{this.GetType().Name}] contains env.json"));
                }
                // do not process other directories
            }
            return names;
        }

    }

    public class ConfigPathResolver
    {
        public static ConfigPathResolver Default { get; } = new ConfigPathResolver();
        private IEnumerable<IConfigFileConvention> Conventions = new[] {
            new EnvJsonConvention()
        };

        public ConfigPathResolver()
        {

        }

        public ConfigPathResolver(IEnumerable<IConfigFileConvention> conventions)
        {
            this.Conventions = conventions;
        }
        public ConfigPathSource[] ExtractNames(string path)
        {
            List<ConfigPathSource> names = new List<ConfigPathSource>();

            foreach (var conv in Conventions)
            {
                var found = conv.GetConfigFiles(path);
                if (found != null) names.AddRange(found);
            }

            return names.ToArray();
        }
    }
}

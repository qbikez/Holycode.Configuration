using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    public interface IConfigFileConvention {
        IEnumerable<ConfigPathSource> GetConfigFiles(string directory);
    }

    public class EnvJsonConvention : IConfigFileConvention {

        public IEnumerable<ConfigPathSource> GetConfigFiles(string directory) {
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            var dirname = Path.GetFileName(directory);
            // search for env.json                
            var files = System.IO.Directory.GetFiles(directory, "env.json");
            if (files.Length > 0)
            {
                foreach(var f in files) {
                    names.Add(new ConfigPathSource(f, $"[{this.GetType().Name}] contains env.json"));
                }
            }
            return names;
        }

    }

    public class ConfigPathResolver
    {
        private static IConfigFileConvention[] Conventions = new[] {
            new EnvJsonConvention()
        };
        public static ConfigPathSource[] ExtractNames(string path)
        {
            int levelUp = 0; // 0 for a path to a file and 1 for directory          
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            for (string dir = path; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir), levelUp++)
            {
                var dirname = Path.GetFileName(dir);

                if (System.IO.Directory.Exists(dir))
                {
                    foreach(var conv in Conventions) {
                        var found = conv.GetConfigFiles(dir);
                        if (found != null) names.AddRange(found);
                    }
                }
            }

            // if (names.Count == 0)
            // {
            //     if (!path.Contains("\\") && !path.Contains("/"))
            //     {
            //         names.Add(new ConfigPathSource(path, path, "direct name"));
            //     }
            // }

            return names.ToArray();
        }
    }
}

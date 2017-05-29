using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    ///<summary>
    /// env.json convention
    ///</summary>
    public class UpwardFileFinder 
    {
        public UpwardFileFinder()
        {
        }

        public IEnumerable<ConfigPathSource> Find(string rootDir, string filePattern, bool stopOnFirstMatch = false) => FindConfigFilesUpwards(rootDir, filePattern, stopOnFirstMatch);
        
        private IEnumerable<ConfigPathSource> FindConfigFilesUpwards(string rootDir, string filePattern, bool stopOnFirstMatch = false)
        {
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            int levelUp = 0; // 0 for a path to a file and 1 for directory          

            for (string dir = rootDir; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir), levelUp++)
            {
                var dirname = Path.GetFileName(dir);

                if (System.IO.Directory.Exists(dir))
                {
                    var found = GetConfigFilesInDir(dir, filePattern);
                    if (found != null && found.Any())
                    {
                        names.AddRange(found);
                        break;
                    }
                }
            }

            return names;
        }

        private IEnumerable<ConfigPathSource> GetConfigFilesInDir(string directory, string filePattern)
        {
            // stop at first env.json
            List<ConfigPathSource> names = new List<ConfigPathSource>();
            var dirname = Path.GetFileName(directory);
            // search for env.json                
            var files = System.IO.Directory.GetFiles(directory, filePattern);
            if (files.Length > 0)
            {
                foreach (var f in files)
                {
                    names.Add(new ConfigPathSource(f, $"[{this.GetType().Name}] contains {filePattern}"));
                }
                // do not process other directories
            }
            return names;
        }

    }

}
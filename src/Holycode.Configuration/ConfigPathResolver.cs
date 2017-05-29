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
            if (string.IsNullOrWhiteSpace(Path.GetExtension(path))) levelUp = 1;

            if (path.Contains('|'))
            {
                var splits = path.Split('|');
                return splits.Select(s => new ConfigPathSource()
                {
                    ServiceName = s,
                    Source = "split"
                }).ToArray();
            }

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

                // this is old method, inherited from "locator" project. don't use it

                // look for www-* or *-www
                //var m1 = Regex.Match(dirname, @"(www-([a-zA-Z1-9\-]*)|([a-zA-Z1-9\-]*)-www)$");
                //if (m1.Success)
                //{
                //    if (!string.IsNullOrEmpty(m1.Groups[2].Value)) names.Add(new ServicePathSource(m1.Groups[2].Value, dir, "wwww-*"));
                //    else if (!string.IsNullOrEmpty(m1.Groups[3].Value)) names.Add(new ServicePathSource(m1.Groups[3].Value, dir, "*-www"));
                //    continue;
                //}

                //// look for xxx_postfix or xxx+posftix
                //var m2 = Regex.Match(dirname, @".*[_+]([a-zA-Z\-]+)$");
                //if (m2.Success)
                //{
                //    if (!string.IsNullOrEmpty(m2.Groups[1].Value)) names.Add(new ServicePathSource(m2.Groups[1].Value, dir, "xxx_+postfix"));
                //    continue;
                //}


                //if (names.Count == 0 || names[0].ServiceName == "bin")
                //{
                //    // look for first descendant of "www" directory
                //    var parent = Path.GetDirectoryName(dir);
                //    if (parent != null)
                //    {
                //        parent = Path.GetFileName(parent);
                //        if (parent != null && parent == "www" && levelUp > 0)
                //        {
                //            names.Add(new ServicePathSource(dirname, dir, "first descendant of 'www' directory"));
                //            continue;
                //        }
                //    }
                //}

                // default path is: c:\...\group-name\svc|tasks|etc\servicename\service.exe
                // or               c:\...\src-name  \bin          \debug      \service.exe
                // levelUp == 3 should match the group-name
                //if ((levelUp == 3)
                //    && names.Count == 0 && dir.Split('\\').Contains("www"))
                //{
                //    names.Add(new ServicePathSource(dirname, dir, @"matches c:\...\group-name\svc|tasks|etc\servicename\service.exe"));
                //    continue;
                //}

                //if (dirname.Contains("-dev") || dirname.Contains("-rel") || dirname.Contains("-qa") || dirname.Contains("-prod"))
                //{
                //    names.Add(new ServicePathSource(dirname, dir, @"*-(dev|rel|qa|prod)"));
                //}


            }

            if (names.Count == 0)
            {
                if (!path.Contains("\\") && !path.Contains("/"))
                {
                    names.Add(new ConfigPathSource(path, path, "direct name"));
                }
            }

            return names.ToArray();
        }
    }
}

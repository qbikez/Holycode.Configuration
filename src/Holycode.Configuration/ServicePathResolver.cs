using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Holycode.Configuration
{
    public class ServicePathResolver
    {
        public static string DEFAULT_DEBUG_CONFIG = "default-debug";
        public static string DEFAULT_RELEASE_CONFIG = "default-release";

        public static string ExtractName(string path = null)
        {
            if (path == null)
            {
                //var locCfgName = System.Configuration.ConfigurationManager.AppSettings["locator-config"];
                //if (!string.IsNullOrEmpty(locCfgName))
                //    path = locCfgName;
                if (path == null)
                {
#if !CORECLR
                    path = Assembly.GetCallingAssembly().CodeBase.Substring("file:///".Length);
#else
                throw new Exception("could not resolve path from calling assembly codebase");
#endif
                }
            }
            var names = ExtractNames(path);
            var name = string.Join("|", names.Select(n => n.ServiceName));

            if (string.IsNullOrEmpty(name))
            {
#if DEBUG
                return DEFAULT_DEBUG_CONFIG;
#else
                return DEFAULT_RELEASE_CONFIG;
#endif
            }

            return name;
        }

        /// <summary>
        /// try to split name by '-' and trim first element
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string ExtractSubName(string path, out string superName)
        {
            superName = null;
            var splits = path.Split('-');
            if (splits.Length <= 1) return null;

            superName = splits[0];
            path = path.Substring(splits[0].Length + 1);

            return path;
        }

        //public static string ExtractName(string path)
        //{
        //    for (string dir = path; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir))
        //    {
        //        var dirname = Path.GetFileName(dir);
        //        var m = Regex.Match(dirname, @"(www-([a-zA-Z1-9\-]*)|([a-zA-Z1-9\-]*)-www)$");
        //        if (m.Success)
        //        {
        //            if (!string.IsNullOrEmpty(m.Groups[2].Value)) return m.Groups[2].Value;
        //            if (!string.IsNullOrEmpty(m.Groups[3].Value)) return m.Groups[3].Value;
        //        }
        //    }

        //    if (!path.Contains("\\") && !path.Contains("/"))
        //        return path;

        //    return null;
        //}

        public static ServicePathSource[] ExtractNames(string path)
        {
            int levelUp = 0; // 0 for a path to a file and 1 for directory
            if (string.IsNullOrWhiteSpace(Path.GetExtension(path))) levelUp = 1;

            if (path.Contains('|'))
            {
                var splits = path.Split('|');
                return splits.Select(s => new ServicePathSource()
                {
                    ServiceName = s,
                    Source = "split"
                }).ToArray();
            }

            List<ServicePathSource> names = new List<ServicePathSource>();
            for (string dir = path; !string.IsNullOrEmpty(dir); dir = Path.GetDirectoryName(dir), levelUp++)
            {
                var dirname = Path.GetFileName(dir);

                // look for www-* or *-www
                var m1 = Regex.Match(dirname, @"(www-([a-zA-Z1-9\-]*)|([a-zA-Z1-9\-]*)-www)$");
                if (m1.Success)
                {
                    if (!string.IsNullOrEmpty(m1.Groups[2].Value)) names.Add(new ServicePathSource(m1.Groups[2].Value, dir, "wwww-*"));
                    else if (!string.IsNullOrEmpty(m1.Groups[3].Value)) names.Add(new ServicePathSource(m1.Groups[3].Value, dir, "*-www"));
                    continue;
                }

                // look for xxx_postfix or xxx+posftix
                var m2 = Regex.Match(dirname, @".*[_+]([a-zA-Z\-]+)$");
                if (m2.Success)
                {
                    if (!string.IsNullOrEmpty(m2.Groups[1].Value)) names.Add(new ServicePathSource(m2.Groups[1].Value, dir, "xxx_+postfix"));
                    continue;
                }


                if (names.Count == 0 || names[0].ServiceName == "bin")
                {
                    // look for first descendant of "www" directory
                    var parent = Path.GetDirectoryName(dir);
                    if (parent != null)
                    {
                        parent = Path.GetFileName(parent);
                        if (parent != null && parent == "www" && levelUp > 0)
                        {
                            names.Add(new ServicePathSource(dirname, dir, "first descendant of 'www' directory"));
                            continue;
                        }
                    }
                }

                // default path is: c:\...\group-name\svc|tasks|etc\servicename\service.exe
                // or               c:\...\src-name  \bin          \debug      \service.exe
                // levelUp == 3 should match the group-name
                if ((levelUp == 3)
                    && names.Count == 0 && dir.Split('\\').Contains("www"))
                {
                    names.Add(new ServicePathSource(dirname, dir, @"matches c:\...\group-name\svc|tasks|etc\servicename\service.exe"));
                    continue;
                }

                if (dirname.Contains("-dev") || dirname.Contains("-rel") || dirname.Contains("-qa") || dirname.Contains("-prod"))
                {
                    names.Add(new ServicePathSource(dirname, dir, @"*-(dev|rel|qa|prod)"));
                }

                if (System.IO.Directory.Exists(dir))
                {
                    var files = System.IO.Directory.GetFiles(dir, "env.json");
                    if (files.Length > 0)
                    {
                        names.Add(new ServicePathSource(dirname, dir, @"contains env.json"));
                    }
                }
            }

            if (names.Count == 0)
            {
                if (!path.Contains("\\") && !path.Contains("/"))
                {
                    names.Add(new ServicePathSource(path, path, "direct name"));
                }
            }

            names.Reverse();
            return names.ToArray();
        }
    }
}
